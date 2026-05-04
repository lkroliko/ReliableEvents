namespace MrRabbit.ReliableEvents;

public readonly struct OutboxQueue : IEquatable<OutboxQueue>
{
    public string Name { get; }
    public OutboxQueue(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Queue name cannot be null or whitespace.", nameof(name));
        Name = name;
    }

    public bool Equals(OutboxQueue other) => Name == other.Name;

    public override bool Equals(object? obj) => obj is OutboxQueue other && Equals(other);

    public override int GetHashCode() => Name?.GetHashCode() ?? 0;

    public static bool operator ==(OutboxQueue left, OutboxQueue right) => left.Equals(right);

    public static bool operator !=(OutboxQueue left, OutboxQueue right) => !left.Equals(right);
}
