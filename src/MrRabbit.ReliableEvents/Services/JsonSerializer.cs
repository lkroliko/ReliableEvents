namespace MrRabbit.ReliableEvents.Services;

internal class JsonSerializer : ISerializer
{
    public string Serialize(object value) =>
        System.Text.Json.JsonSerializer.Serialize(value);

    public object? Deserialize(string json, Type type)
        => System.Text.Json.JsonSerializer.Deserialize(json, type);
}
