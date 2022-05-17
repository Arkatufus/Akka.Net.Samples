using Akka.Actor;

namespace Sample.Shared
{
    public sealed class Ping
    {
        public Ping(IActorRef sender, string payload)
        {
            Sender = sender;
            Payload = payload;
        }

        public IActorRef Sender { get; }
        public string Payload { get; }
    }

    public sealed class Pong
    {
        public Pong(string payload)
        {
            Payload = payload;
        }

        public string Payload { get; }
    }
}