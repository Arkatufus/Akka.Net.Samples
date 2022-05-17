using System;
using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Event;
using Sample.Shared;

namespace Sample.Client
{
    internal sealed class SendPing
    {
        public static readonly SendPing Instance = new SendPing();
        private SendPing() { }
    }
    
    public class ClientActor: ReceiveActor, IWithTimers
    {
        private const string TimerName = "client-send";
        private readonly Random _rnd = new Random(1234);
        private IActorRef _clusterClient;

        public ClientActor()
        {
            var log = Context.GetLogger();
            
            Receive<SendPing>( _ =>
            {
                var actorPath = Util.ActorPath(_rnd.Next(0, Consts.TotalActors));
                log.Info($"Sending ping to {actorPath}");
                _clusterClient.Tell(new ClusterClient.Send(actorPath, new Ping(Self, "PING!")));
            });
            
            Receive<Pong>(pong =>
            {
                log.Info($"Received pong: {pong.Payload}");
            });
        }
        
        protected override void PreStart()
        {
            _clusterClient = Context.ActorOf(ClusterClient.Props(
                ClusterClientSettings.Create(Context.System).WithInitialContacts(Consts.ContactPoints)));
            
            Timers.StartPeriodicTimer(TimerName, SendPing.Instance, TimeSpan.FromSeconds(1));
        }

        protected override void PostStop()
        {
            Timers.CancelAll();
        }

        public ITimerScheduler Timers { get; set; }
    }
}