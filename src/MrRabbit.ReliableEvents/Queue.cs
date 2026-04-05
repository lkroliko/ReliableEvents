namespace MrRabbit.ReliableEvents;

public struct Queue : IEquatable<Queue>
{
    public string Name { get; }
    public Queue(string name)
    {
        Name = name;
    }

    public bool Equals(Queue other) => Name == other.Name;

    public override bool Equals(object? obj) => obj is Queue other && Equals(other);

    public override int GetHashCode() => Name?.GetHashCode() ?? 0;

    public static bool operator ==(Queue left, Queue right) => left.Equals(right);

    public static bool operator !=(Queue left, Queue right) => !left.Equals(right);
}
