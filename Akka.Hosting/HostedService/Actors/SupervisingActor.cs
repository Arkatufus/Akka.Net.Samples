using Akka.Actor;
using Akka.DependencyInjection;

namespace HostedService.Actors;

public class SupervisingActor: ReceiveActor
{
    public static Props Props() => Akka.Actor.Props.Create(() => new SupervisingActor());
        
    private readonly DependencyResolver _resolver;
    private readonly Dictionary<string, IActorRef> _processorDict = new Dictionary<string, IActorRef>();
    
    public SupervisingActor()
    {
        _resolver = DependencyResolver.For(Context.System);

        Receive<Message>(msg =>
        {
            if (!_processorDict.TryGetValue(msg.Id, out var processor))
            {
                processor = Context.ActorOf(_resolver.Props<ProcessorActor>(msg.Id), $"processor-{msg.Id}");
                _processorDict[msg.Id] = processor;
            }

            processor.Forward(msg);
        });
    }
}