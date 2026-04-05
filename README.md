<p align="center">
  <h1 align="center">ReliableEvents</h1>
  <p align="center">
    A lightweight, in memory event dispatcher and transactional outbox pattern implementation for .NET — guaranteeing reliable domain event delivery with Entity Framework Core.
  </p>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#installation">Installation</a> •
  <a href="#quick-start">Quick Start</a> •
  <a href="#architecture">Architecture</a> •
  <a href="#api-reference">API Reference</a> •
  <a href="#license">License</a>
</p>

---

## Overview

**ReliableEvents** solves the dual-write problem in distributed systems. When your application needs to persist state *and* publish events, traditional approaches risk data inconsistency — the database commits but the event is lost, or vice versa.

This library implements the **Transactional Outbox Pattern**: events are serialized and stored in the same database transaction as your domain state, then dispatched asynchronously by a dedicated worker. This guarantees **at-least-once delivery** with zero external dependencies beyond your existing EF Core database.

> **⚠️ Important:** This library is intended for applications running as a single instance connected to an owned database. It does not support horizontal scaling or competing consumers. If you run multiple instances of your application against the same database, outbox events may be processed more than once.

## Features

- 🔒 **Transactional safety** — events are persisted atomically with your domain state via EF Core
- 📬 **Outbox pattern** — automatic serialization, storage, and dispatching of outbox events
- ⚡ **In-memory dispatcher** — lightweight, synchronous event dispatch for in-process handlers
- 🔀 **Named queues** — isolate event processing across independent queues with `[EventHandlerQueue]`
- 🛡️ **Concurrency control** — per-queue semaphores prevent duplicate processing
- 📦 **Idempotency support** — optional `EventId` tracking to mark events as dispatched instead of deleting
- 🧩 **Minimal setup** — single `AddReliableEvents<TDbContext>()` call with a fluent builder API
- 🎯 **Convention-based registration** — auto-discover handlers from assemblies

## Installation

> **Requirements:** .NET 10+, Entity Framework Core 10+

```shell
dotnet add package MrRabbit.ReliableEvents
```

## Quick Start

### 1. Define your events

```csharp
// In-memory domain event (fire-and-forget within the process)
public class OrderPlaced
{
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
}

// Outbox event (persisted & guaranteed delivery)
public class OrderConfirmed
{
    public Guid OrderId { get; init; }
    public DateTime ConfirmedAt { get; init; }
}
```

### 2. Implement handlers

**In-memory event handler** — executed immediately when dispatched:

```csharp
public class OrderPlacedHandler : IEventHandler<OrderPlaced>
{
    public Task HandleAsync(OrderPlaced @event, CancellationToken cancellationToken)
    {
        // Handle the event in-process
        Console.WriteLine($"Order {@@event.OrderId} placed for {@@event.Total:C}");
        return Task.CompletedTask;
    }
}
```

**Outbox event handler** — executed reliably from the outbox after persistence. Each handler runs in its own scope and the dispatcher automatically calls `SaveChanges` on DbContext after successful execution to mark the task as processed — you do **not** need to call it yourself:

```csharp
[EventHandlerQueue("orders")]
public class OrderConfirmedHandler : IOutboxEventHandler<OrderConfirmed>
{
    public Task HandleAsync(OrderConfirmed @event, CancellationToken cancellationToken)
    {
        // This handler is guaranteed to execute at least once.
        // SaveChanges is called by the dispatcher after this method completes —
        // the OutboxTask is marked as dispatched (or removed) automatically.
        Console.WriteLine($"Order {@@event.OrderId} confirmed at {@@event.ConfirmedAt}");
        return Task.CompletedTask;
    }
}
```

### 3. Configure your DbContext

```csharp
public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddReliableEvents(); // Registers the OutboxTask entity
    }
}
```

### 4. Register services

```csharp
builder.Services.AddReliableEvents<AppDbContext>(options =>
{
    options.AddEventHandlers(typeof(Program).Assembly);
    options.AddOutboxEventHandlers(typeof(Program).Assembly);
});
```

### 5. Use in your application

```csharp
public class OrderService
{
    private readonly IReliableEvents<AppDbContext> _events;
    private readonly AppDbContext _dbContext;

    public OrderService(IReliableEvents<AppDbContext> events, AppDbContext dbContext)
    {
        _events = events;
        _dbContext = dbContext;
    }

    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        _dbContext.Orders.Add(order);

        // Fire in-memory events (immediate, no persistence)
        await _events.Dispatcher.DispatchAsync(
            [new OrderPlaced { OrderId = order.Id, Total = order.Total }], ct);

        // Attach outbox events (persisted with SaveChanges)
        var queues = _events.OutboxStore.AttachEvent(
            new OrderConfirmed { OrderId = order.Id, ConfirmedAt = DateTime.UtcNow },
            eventId: order.Id,
            occurredDate: DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(ct);

        // Dispatch outbox events from the persisted queue
        await _events.OutboxDispatcher.DispatchAsync(queues);
    }
}
```

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        IReliableEvents<TDbContext>              │
│                                                                 │
│  ┌──────────────┐  ┌──────────────────┐  ┌───────────────────┐  │
│  │  IDispatcher │  │  IOutboxStore    │  │ IOutboxDispatcher │  │
│  │              │  │                  │  │                   │  │
│  │  In-memory   │  │  Serializes &    │  │  Reads & executes │  │
│  │  event fan-  │  │  persists events │  │  outbox tasks per │  │
│  │  out to all  │  │  as OutboxTasks  │  │  queue with       │  │
│  │  registered  │  │  within the EF   │  │  semaphore-based  │  │
│  │  IEventHan-  │  │  transaction     │  │  concurrency      │  │
│  │  dler<T>     │  │                  │  │  control          │  │
│  └──────────────┘  └──────────────────┘  └───────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Core Components

| Component | Interface | Scope | Description |
|---|---|---|---|
| **ReliableEvents** | `IReliableEvents<TDbContext>` | Scoped | The main entry point — aggregates `IDispatcher`, `IOutboxStore`, and `IOutboxDispatcher` into a single injectable service. |
| **Dispatcher** | `IDispatcher` | Singleton | Resolves and invokes all `IEventHandler<T>` implementations for a given set of events in-memory. |
| **Outbox Store** | `IOutboxStore<TDbContext>` | Scoped | Serializes events and attaches them as `OutboxTask` entities to the current EF Core change tracker. Events are committed with your `SaveChanges` call. |
| **Outbox Dispatcher** | `IOutboxDispatcher<TDbContext>` | Scoped | Processes all queues in parallel. Each queue is processed sequentially (oldest-first) with per-queue semaphore locking. |

### Event Lifecycle

```
  Domain Event                         Outbox Event
  ───────────                          ────────────
       │                                    │
       ▼                                    ▼
  IDispatcher.DispatchAsync()     IOutboxStore.AttachEvent()
       │                                    │
       ▼                                    ▼
  IEventHandler<T>.HandleAsync()   DbContext.SaveChanges()
  (immediate, in-process)           (persisted as OutboxTask)
                                            │
                                            ▼
                                   IOutboxDispatcher.DispatchAsync()
                                            │
                                            ▼
                                   IOutboxEventHandler<T>.HandleAsync()
                                   (reliable, at-least-once)
```

## API Reference

### Registration

#### `AddReliableEvents<TDbContext>(Action<ReliableEventsOptionsBuilder>)`

Registers all ReliableEvents services into the DI container.

```csharp
services.AddReliableEvents<AppDbContext>(options =>
{
    // Auto-discover all IEventHandler<T> in given assemblies
    options.AddEventHandlers(typeof(Program).Assembly);

    // Auto-discover all IOutboxEventHandler<T> in given assemblies
    options.AddOutboxEventHandlers(typeof(Program).Assembly);

    // Or register individual handlers
    options.AddEventHandler(typeof(OrderPlacedHandler));
    options.AddOutboxEventHandler(typeof(OrderConfirmedHandler));
});
```

#### `ModelBuilder.AddReliableEvents()`

Applies the `OutboxTask` entity configuration to your EF Core model.

```csharp
modelBuilder.AddReliableEvents();
```

### Interfaces

#### `IEventHandler<TEvent>`

Implement for in-memory (non-persistent) event handling.

```csharp
public interface IEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
```

#### `IOutboxEventHandler<TEvent>`

Implement for reliable, outbox-backed event handling. Must be decorated with `[EventHandlerQueue]`.

```csharp
public interface IOutboxEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
```

### Attributes

#### `[EventHandlerQueue(string queueName)]`

Assigns an outbox handler to a named queue. Events in the same queue are processed sequentially (FIFO). Different queues are processed in parallel.

```csharp
[EventHandlerQueue("notifications")]
public class EmailNotificationHandler : IOutboxEventHandler<OrderConfirmed> { ... }
```

### `IReliableEvents<TDbContext>`

The main entry point, providing access to all three subsystems:

| Property | Type | Description |
|---|---|---|
| `Dispatcher` | `IDispatcher` | In-memory event dispatcher |
| `OutboxStore` | `IOutboxStore<TDbContext>` | Attaches events to the EF Core transaction |
| `OutboxDispatcher` | `IOutboxDispatcher<TDbContext>` | Dispatches persisted outbox events |

### `IOutboxStore<TDbContext>`

| Method | Description |
|---|---|
| `AttachEvent(object, Guid?, DateTime)` | Attaches a single event to the outbox. Returns affected `OutboxQueue`s. |
| `AttachEvents<T>(IEnumerable<T>, Func<T,Guid?>, Func<T,DateTime>)` | Attaches multiple events with custom ID and timestamp factories. |

### `IOutboxDispatcher<TDbContext>`

| Method | Description |
|---|---|
| `DispatchAsync(IEnumerable<OutboxQueue>)` | Dispatches all pending tasks across the given queues. Returns `DispatchResult[]`. |

### `DispatchResult`

| Property | Type | Description |
|---|---|---|
| `Queue` | `OutboxQueue?` | The queue this result belongs to |
| `IsSuccess` | `bool` | Whether dispatching succeeded |
| `IsFailed` | `bool` | Inverse of `IsSuccess` |
| `Exception` | `Exception?` | The exception if dispatching failed |

### `OutboxTask` Entity

Persisted to your database via EF Core. Indexed on `(QueueName, IsDispatched, OccurredDate)`.

| Column | Type | Description |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `QueueName` | `string` | Queue this task belongs to |
| `EventId` | `Guid?` | Optional business event ID for idempotency |
| `HandlerFullName` | `string` | Fully qualified handler type name |
| `HandlerAssemblyName` | `string` | Handler assembly name |
| `EventFullName` | `string` | Fully qualified event type name |
| `EventAssemblyName` | `string` | Event assembly name |
| `EventData` | `string` | JSON-serialized event payload |
| `OccurredDate` | `DateTime` | When the event occurred |
| `IsDispatched` | `bool` | Whether the task has been processed |

## Key Concepts

### Idempotency via EventId

When you provide an `EventId`, dispatched tasks are **marked as dispatched** rather than deleted. This enables idempotency checks and audit trails:

```csharp
// With EventId — task marked as dispatched after processing
outboxStore.AttachEvent(myEvent, eventId: Guid.NewGuid(), occurredDate: DateTime.UtcNow);

// Without EventId — task deleted after processing
outboxStore.AttachEvent(myEvent, eventId: null, occurredDate: DateTime.UtcNow);
```

### Queue Isolation & Concurrency

Each named queue is processed independently with its own semaphore. This means:

- **Within a queue** — tasks are processed sequentially in FIFO order (oldest first)
- **Across queues** — processing happens in parallel
- **Concurrent dispatch calls** — the semaphore ensures only one consumer processes a queue at a time, preventing duplicate handling

```csharp
[EventHandlerQueue("payments")]    // Processed independently...
public class PaymentHandler : IOutboxEventHandler<PaymentEvent> { ... }

[EventHandlerQueue("notifications")] // ...from this queue
public class NotificationHandler : IOutboxEventHandler<NotificationEvent> { ... }
```

### Error Handling & Queue Blocking

If an `IOutboxEventHandler<T>` throws an exception, the **entire queue stops processing**. The failing task is **not** marked as dispatched, so it remains at the head of the queue. The `DispatchResult` returned for that queue will have `IsFailed = true` with the captured `Exception`. Subsequent calls to `DispatchAsync` will retry the same task, effectively blocking the queue until the issue is resolved.

> **💡 Tip:** Since a failing handler blocks all subsequent tasks in the same queue, keep handler logic resilient (e.g. wrap external calls in try/catch with logging) or isolate critical handlers into separate queues so a failure in one does not stall others.

## License

This project is licensed under the [MIT License](LICENSE).