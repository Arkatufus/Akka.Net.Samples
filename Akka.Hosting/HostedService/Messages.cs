namespace HostedService;

public sealed class Message
{
    public Message(string id, object payload)
    {
        Id = id;
        Payload = payload;
    }

    public string Id { get; }
    public object Payload { get; }
}