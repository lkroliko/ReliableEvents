namespace MrRabbit.ReliableEvents.Interfaces;

public interface ISerializer
{
    string Serialize(object value);
    object? Deserialize(string json, Type type);
}
