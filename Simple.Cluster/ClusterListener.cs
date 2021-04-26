using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;

namespace Simple.Cluster
{
    public class ClusterListener : ReceiveActor
    {
        public static Props Props() => Akka.Actor.Props.Create(() => new ClusterListener());

        private readonly ILoggingAdapter _log;

        public ClusterListener()
        {
            _log = Context.GetLogger();

            var cluster = Akka.Cluster.Cluster.Get(Context.System);
            cluster.Subscribe(
                Self, 
                ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents, 
                typeof(ClusterEvent.MemberStatusChange),
                typeof(ClusterEvent.ReachabilityEvent));

            Receive<ClusterEvent.ReachabilityEvent>(message =>
            {
                switch (message)
                {
                    case ClusterEvent.UnreachableMember msg:
                        _log.Info($"Member detected as unreachable: {msg.Member}");
                        break;
                    case ClusterEvent.ReachableMember msg:
                        _log.Info($"Member is now reachable: {msg.Member}");
                        break;
                    default:
                        Unhandled(message);
                        break;
                }
            });

            Receive<ClusterEvent.MemberStatusChange>(message =>
            {
                switch (message)
                {
                    case ClusterEvent.MemberUp msg:
                        _log.Info($"Member is now Up: {msg.Member.Address}");
                        break;
                    case ClusterEvent.MemberRemoved msg:
                        _log.Info($"Member is removed: {msg.Member.Address} after {msg.PreviousStatus}");
                        break;
                    default:
                        Unhandled(message);
                        break;
                }
            });
        }
    }
}
