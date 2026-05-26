<p align="center">
  <h1 align="center">ReliableEvents</h1>
  <p align="center">
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-10-512bd4" alt=".NET 10"></a>
  <a href="https://www.nuget.org/packages/ReliableEvents"><img src="https://img.shields.io/nuget/v/ReliableEvents?logo=nuget&color=004880" alt="NuGet"></a>
  <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License: MIT"></a>
</p>
  <p align="center">
    A lightweight, in memory event dispatcher and transactional outbox pattern implementation for .NET — guaranteeing reliable domain event delivery with Entity Framework Core.
  </p>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#installation">Installation</a> •
  <a href="#quick-start">Quick Start</a> •
  <a href="#ddd-integration">DDD Integration</a> •
  <a href="#architecture">Architecture</a> •
  <a href="#api-reference">API Reference</a> •
  <a href="CHANGELOG.md">Changelog</a> •
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
- 📦 **External event deduplication** — optional `EventId` to detect and skip duplicate events received from external systems (e.g. SignalR, webhooks)
- 🪝 **Dispatch handlers** — plug into the outbox lifecycle with `IOutboxDispatchedQueueHandler` and `IOutboxDispatchQueueErrorHandler`
- 🧩 **Minimal setup** — single `AddReliableEvents<TDbContext>()` call with a fluent builder API
- 🎯 **Convention-based registration** — auto-discover handlers from assemblies

## Installation

> **Requirements:** .NET 10+, Entity Framework Core 10+

```shell
dotnet add package MrRabbit.ReliableEvents
```

### Tested Databases

The library is end-to-end tested against the following database providers:

| Database | EF Core Provider | Version |
|---|---|---|
| SQLite | `Microsoft.EntityFrameworkCore.Sqlite` | — |
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` | 2025 |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` | 18 |

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
            eventId: null,
            occurredDate: DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(ct);

        // Dispatch outbox events from the persisted queue
        await _events.OutboxDispatcher.DispatchAsync(queues);
    }
}
```

### 6. Implement a recurring dispatch job

The library does **not** include a built-in background worker. You must implement a recurring task (e.g. using `IHostedService`, Hangfire, Quartz, or a simple timer) that periodically calls `DispatchAsync` on `IOutboxDispatcher<TDbContext>` to retry failed or unprocessed events:

```csharp
public class OutboxDispatcherJob : BackgroundService
{
    private readonly IOutboxDispatcher<AppDbContext> _outboxDispatcher;

    public OutboxDispatcherJob(IOutboxDispatcher<AppDbContext> outboxDispatcher)
    {
        _outboxDispatcher = outboxDispatcher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Retry dispatching for all queues discovered from the database
            await _outboxDispatcher.DispatchAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

### 7. Implement cleanup of dispatched tasks

> **💡 Note:** This step is only necessary if you use `EventId` (i.e. when processing events from external systems). Outbox tasks created without an `EventId` are automatically deleted after successful dispatch — no cleanup is needed for them.

The library does **not** delete outbox tasks that have an `EventId` — they are only marked as dispatched to enable [deduplication](#eventid--external-event-deduplication). You must implement a periodic cleanup job to remove old, already-processed records and prevent the outbox table from growing indefinitely:

```csharp
public class OutboxCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxCleanupJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Remove dispatched tasks older than 7 days
            var cutoff = DateTime.UtcNow.AddDays(-7);
            await dbContext.Set<OutboxTask>()
                .Where(t => t.IsDispatched && t.OccurredDate < cutoff)
                .ExecuteDeleteAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

## DDD Integration

In Domain-Driven Design, domain events are raised within your aggregates and dispatched as part of the persistence lifecycle. ReliableEvents integrates naturally with this approach by intercepting `SaveChanges` in your `DbContext` to automatically:

1. **Dispatch domain events** in-memory via `IDispatcher`
2. **Persist integration events** to the outbox via `IOutboxStore` and dispatch them via `IOutboxDispatcher`

Integration events are domain events marked with the `IIntegrationEvent` interface — they cross bounded context boundaries and require guaranteed delivery.

### 1. Define the marker interface

```csharp
public interface IIntegrationEvent;
```

### 2. Define a base class for domain events

```csharp
public abstract class DomainEvent
{
    public DateTime OccurredDate { get; } = DateTime.UtcNow;
}
```

### 3. Define your domain events

```csharp
// In-memory only — handled within the same bounded context
public class OrderPlaced : DomainEvent
{
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
}

// Integration event — persisted to outbox, guaranteed delivery across boundaries
public class OrderConfirmed : DomainEvent, IIntegrationEvent
{
    public Guid OrderId { get; init; }
}
```

### 4. Collect events in your aggregate

```csharp
public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public class Order : AggregateRoot
{
    public Guid Id { get; private set; }
    public decimal Total { get; private set; }

    public static Order Place(Guid id, decimal total)
    {
        var order = new Order { Id = id, Total = total };
        order.RaiseDomainEvent(new OrderPlaced { OrderId = id, Total = total });
        return order;
    }

    public void Confirm()
    {
        RaiseDomainEvent(new OrderConfirmed { OrderId = Id });
    }
}
```

### 5. Override SaveChanges to dispatch events

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddReliableEvents();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var reliableEvents = this.GetService<IReliableEvents<AppDbContext>>();

        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();

        // Separate domain events from integration events
        var inMemoryEvents = domainEvents.Where(e => e is not IIntegrationEvent).ToList();
        var integrationEvents = domainEvents.Where(e => e is IIntegrationEvent).ToList();

        // Attach integration events to the outbox (persisted with this SaveChanges call)
        var queues = reliableEvents.OutboxStore.AttachEvents(integrationEvents, _ => null, e => e.OccurredDate);

        // Dispatch in-memory domain events
        await reliableEvents.Dispatcher.DispatchAsync(inMemoryEvents, cancellationToken);

        aggregates.ForEach(a => a.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch outbox events after successful persistence
        Task.Run(() => reliableEvents.OutboxDispatcher.DispatchAsync(queues)).ConfigureAwait(false);

        return result;
    }
}
```

### 6. Use it — your application code stays clean

```csharp
public class OrderService
{
    private readonly AppDbContext _dbContext;

    public OrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task PlaceAndConfirmAsync(Guid orderId, decimal total, CancellationToken ct)
    {
        var order = Order.Place(orderId, total);
        order.Confirm();

        _dbContext.Orders.Add(order);

        // SaveChanges automatically:
        // 1. Dispatches OrderPlaced in-memory
        // 2. Persists OrderConfirmed to the outbox
        // 3. Dispatches OrderConfirmed from the outbox
        await _dbContext.SaveChangesAsync(ct);
    }
}
```

> **💡 Note:** Steps 6 and 7 from [Quick Start](#quick-start) (recurring dispatch job and cleanup of dispatched tasks) still apply — make sure to implement them.

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
| **Dispatcher** | `IDispatcher` | Scoped | Resolves and invokes all `IEventHandler<T>` implementations for a given set of events in-memory. |
| **Outbox Store** | `IOutboxStore<TDbContext>` | Scoped | Serializes events and attaches them as `OutboxTask` entities to the current EF Core change tracker. Events are committed with your `SaveChanges` call. |
| **Outbox Dispatcher** | `IOutboxDispatcher<TDbContext>` | Singleton | Processes all queues in parallel. Each queue is processed sequentially (oldest-first) with per-queue semaphore locking. |
| **Outbox Store Statistics** | `IOutboxStoreStatistics<TDbContext>` | Scoped | Provides statistics about waiting (not yet dispatched) outbox tasks, grouped by queue. |

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

    // Register dispatch lifecycle handlers
    options.AddOutboxDispatchedQueueHandler<MyDispatchedHandler>();
    options.AddOutboxDispatchQueueErrorHandler<MyDispatchErrorHandler>();
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

#### `IOutboxDispatchedQueueHandler`

Implement to execute logic **after** a queue has been successfully dispatched (i.e. at least one outbox task was processed). The handler receives an `OutboxDispatchedQueueContext` containing the queue and the number of dispatched tasks. Multiple handlers can be registered — they are invoked sequentially.

```csharp
public interface IOutboxDispatchedQueueHandler
{
    Task HandleAsync(OutboxDispatchedQueueContext context);
}
```

**`OutboxDispatchedQueueContext`**

| Property | Type | Description |
|---|---|---|
| `Queue` | `OutboxQueue` | The queue that was dispatched |
| `DispatchedTasksCount` | `int` | Number of tasks successfully dispatched in this run |

**Example:**

```csharp
public class LoggingDispatchedHandler : IOutboxDispatchedQueueHandler
{
    private readonly ILogger<LoggingDispatchedHandler> _logger;

    public LoggingDispatchedHandler(ILogger<LoggingDispatchedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OutboxDispatchedQueueContext context)
    {
        _logger.LogInformation(
            "Queue '{Queue}' dispatched {Count} task(s).",
            context.Queue.Name, context.DispatchedTasksCount);
        return Task.CompletedTask;
    }
}
```

#### `IOutboxDispatchQueueErrorHandler`

Implement to execute logic when an `IOutboxEventHandler<T>` throws an exception during dispatch. The handler receives an `OutboxDispatchQueueErrorContext` containing the queue and the exception. This is invoked **before** the `DispatchResult` is returned to the caller. Multiple handlers can be registered — they are invoked sequentially.

```csharp
public interface IOutboxDispatchQueueErrorHandler
{
    Task HandleAsync(OutboxDispatchQueueErrorContext context);
}
```

**`OutboxDispatchQueueErrorContext`**

| Property | Type | Description |
|---|---|---|
| `Queue` | `OutboxQueue` | The queue where the exception occurred |
| `Exception` | `Exception` | The exception thrown by the handler |

**Example:**

```csharp
public class AlertingDispatchErrorHandler : IOutboxDispatchQueueErrorHandler
{
    private readonly ILogger<AlertingDispatchErrorHandler> _logger;

    public AlertingDispatchErrorHandler(ILogger<AlertingDispatchErrorHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OutboxDispatchQueueErrorContext context)
    {
        _logger.LogError(
            context.Exception,
            "Dispatch failed for queue '{Queue}'.",
            context.Queue.Name);
        return Task.CompletedTask;
    }
}
```

#### `IOutboxStoreStatistics<TDbContext>`

Provides statistics about waiting (not yet dispatched) outbox tasks. Inject this interface to monitor the outbox queue backlog.

```csharp
public interface IOutboxStoreStatistics<TDbContext> where TDbContext : DbContext
{
    Task<WaitingTaskStatistics> GetWaitingTaskStatisticsAsync();
}
```

**`WaitingTaskStatistics`**

| Property | Type | Description |
|---|---|---|
| `WaitingTasksCount` | `int` | Total number of waiting (not dispatched) outbox tasks across all queues |
| `Queues` | `IEnumerable<QueueWaitingTaskStatistics>` | Per-queue breakdown of waiting tasks |

> **💡 Note:** `WaitingTaskStatistics` defines an implicit conversion to `int`, returning `WaitingTasksCount`.

**`QueueWaitingTaskStatistics`**

| Property | Type | Description |
|---|---|---|
| `Queue` | `OutboxQueue` | The queue |
| `WaitingTasksCount` | `int` | Number of waiting tasks in this queue |

**Example:**

```csharp
public class OutboxHealthCheck
{
    private readonly IOutboxStoreStatistics<AppDbContext> _statistics;

    public OutboxHealthCheck(IOutboxStoreStatistics<AppDbContext> statistics)
    {
        _statistics = statistics;
    }

    public async Task CheckAsync(CancellationToken ct)
    {
        var stats = await _statistics.GetWaitingTaskStatisticsAsync();

        Console.WriteLine($"Total waiting tasks: {stats.WaitingTasksCount}");

        foreach (var queue in stats.Queues)
        {
            Console.WriteLine($"  Queue '{queue.Queue.Name}': {queue.WaitingTasksCount} waiting");
        }
    }
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
| `AttachEvent(object, string?, DateTime)` | Attaches a single event to the outbox. Returns affected `OutboxQueue`s. |
| `AttachEvents<T>(IEnumerable<T>, Func<T,string?>, Func<T,DateTime>)` | Attaches multiple events with custom ID and timestamp factories. |
| `TryAddEventAsync(object, string, DateTime, CancellationToken)` | Attaches a single event and immediately calls `SaveChangesAsync` to persist it. The `eventId` is **required**. If a duplicate `EventId` is detected (either before or during save), returns an **empty** collection — no exception is thrown. |
| `AddEventsAsync<T>(IEnumerable<T>, Func<T,string>, Func<T,DateTime>, CancellationToken)` | Attaches multiple events and immediately calls `SaveChangesAsync` to persist them. The `eventIdFactory` must return a non-null ID. Events whose `EventId` already exists in the database are **skipped**, but if a duplicate key conflict occurs during save, an `OutboxEventAlreadyExistException` is thrown. |

### `IOutboxDispatcher<TDbContext>`

| Method | Description |
|---|---|
| `DispatchAsync(IEnumerable<OutboxQueue>, CancellationToken)` | Dispatches all pending tasks across the given queues. Returns `DispatchResult[]`. |
| `DispatchAsync(CancellationToken)` | Auto-discovers all queues from the database and dispatches all pending tasks. Returns `DispatchResult[]`. |

### `DispatchResult`

| Property | Type | Description |
|---|---|---|
| `Queue` | `OutboxQueue?` | The queue this result belongs to |
| `IsSuccess` | `bool` | Whether dispatching succeeded |
| `IsFailed` | `bool` | Inverse of `IsSuccess` |
| `Exception` | `Exception?` | The exception if dispatching failed |

### `OutboxTask` Entity

Persisted to your database via EF Core. Indexed on `(QueueName, IsDispatched, OccurredDate)` and with a **unique index** on `EventId`.

| Column | Type | Description |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `QueueName` | `string` | Queue this task belongs to |
| `EventId` | `string?` | Optional external event ID for deduplication (see [EventId & External Event Deduplication](#eventid--external-event-deduplication)) |
| `HandlerFullName` | `string` | Fully qualified handler type name |
| `HandlerAssemblyName` | `string` | Handler assembly name |
| `EventFullName` | `string` | Fully qualified event type name |
| `EventAssemblyName` | `string` | Event assembly name |
| `EventData` | `string` | JSON-serialized event payload |
| `OccurredDate` | `DateTime` | When the event occurred |
| `IsDispatched` | `bool` | Whether the task has been processed |

## Key Concepts

### EventId & External Event Deduplication

Idempotency for events raised **within your application** is already guaranteed by the database transaction — the event and your domain state are persisted atomically, so there is no risk of duplicates. In this case, pass `eventId: null`:

```csharp
// Internal event — no EventId needed, transaction guarantees idempotency
outboxStore.AttachEvent(myEvent, eventId: null, occurredDate: DateTime.UtcNow);
```

`EventId` is useful when the event originates from an **external system** (e.g. a webhook, SignalR message, or an HTTP endpoint) and may arrive multiple times due to IO errors or retries. By providing the external system's unique identifier as `EventId`, the outbox task is **marked as dispatched** rather than deleted after processing. This allows the library to detect and skip duplicates if the same external event is received again:

```csharp
// External event — use the external system's ID to prevent duplicate processing
outboxStore.AttachEvent(incomingEvent, eventId: incomingEvent.ExternalId, occurredDate: DateTime.UtcNow);
```

| `EventId` value | After processing | Use case |
|---|---|---|
| `null` | Task is **deleted** | Events raised internally by your application |
| `Guid` | Task is **marked as dispatched** | Events received from external systems that may retry |

### Queue Isolation & Concurrency

Each named queue is processed independently with its own semaphore. This means:

- **Within a queue** — tasks are processed sequentially in FIFO order (oldest first)
- **Across queues** — processing happens in parallel
- **Concurrent dispatch calls** — the semaphore ensures only one consumer processes a queue at a time, preventing duplicate handling

```csharp
[EventHandlerQueue("payments")]
public class PaymentHandler : IOutboxEventHandler<PaymentEvent> { ... }

[EventHandlerQueue("notifications")]
public class NotificationHandler : IOutboxEventHandler<NotificationEvent> { ... }
```

### Error Handling & Queue Blocking

If an `IOutboxEventHandler<T>` throws an exception, the **entire queue stops processing**. The failing task is **not** marked as dispatched, so it remains at the head of the queue. The `DispatchResult` returned for that queue will have `IsFailed = true` with the captured `Exception`. Subsequent calls to `DispatchAsync` will retry the same task, effectively blocking the queue until the issue is resolved.

> **💡 Tip:** Since a failing handler blocks all subsequent tasks in the same queue, keep handler logic resilient (e.g. wrap external calls in try/catch with logging) or isolate critical handlers into separate queues so a failure in one does not stall others.

### Dispatch Handlers

Dispatch handlers let you plug into the outbox dispatcher lifecycle without modifying event handler logic. Two handler interfaces are available:

| Handler | When it runs | Use cases |
|---|---|---|
| `IOutboxDispatchedQueueHandler` | After a queue finishes dispatching (at least one task was processed successfully) | Logging, metrics, triggering downstream workflows |
| `IOutboxDispatchQueueErrorHandler` | When an outbox event handler throws an exception (before the `DispatchResult` is returned) | Alerting, error logging, dead-letter tracking |

Handlers are registered via the builder API:

```csharp
services.AddReliableEvents<AppDbContext>(options =>
{
    options.AddOutboxEventHandlers(typeof(Program).Assembly);
    options.AddOutboxDispatchedQueueHandler<LoggingDispatchedHandler>();
    options.AddOutboxDispatchQueueErrorHandler<AlertingDispatchErrorHandler>();
});
```

Multiple handlers of the same type can be registered — they are invoked sequentially in registration order. If a handler itself throws an exception, it is wrapped in a `ReliableEventsException`.

## License

This project is licensed under the [MIT License](LICENSE.txt).