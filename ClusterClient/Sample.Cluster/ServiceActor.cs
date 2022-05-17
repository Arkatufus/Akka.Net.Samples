using Akka.Actor;
using Akka.Event;
using Sample.Shared;

namespace Sample.Cluster
{
    public class ServiceActor: ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly int _id;
        
        public ServiceActor(int id)
        {
            _id = id;
            _log = Context.GetLogger();

            Receive<Ping>(ping =>
            {
                _log.Info($"Received ping from {ping.Sender}");
                ping.Sender.Tell( new Pong($"From {Util.ActorName(id)}: {ping.Payload} PONG!") );
            });
        }
    }
}