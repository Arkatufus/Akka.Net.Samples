using System;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using PubSub.Messages;
using Publish = PubSub.Messages.Publish;
using PubSubPublish = Akka.Cluster.Tools.PublishSubscribe.Publish;

namespace PubSub.Client
{
    public class ChatClient : ReceiveActor
    {
        public static Props Props(string name) => Akka.Actor.Props.Create(() => new ChatClient(name));
        
        public ChatClient(string name)
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe(Configuration.SystemName, Self));
            Console.WriteLine($"{name} joined chat room");

            Receive<Publish>(msg =>
            {
                mediator.Tell(new PubSubPublish(Configuration.SystemName, new Message(msg.Id, name, msg.Message)));
            });

            Receive<Message>(msg =>
            {
                var direction = Sender.Equals(Self) ? ">>>>" : $"<< {msg.From}:";
                Console.WriteLine($"[{msg.Id}] {name} {direction} {msg.Text}");
            });
        }
    }
}