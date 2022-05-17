using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Cluster.Tools.Client;
using Akka.Cluster.Tools.PublishSubscribe;
using Sample.Shared;

namespace Sample.Cluster
{
    public class ClusterNode
    {
        private readonly int _nodeId;
        private readonly int _port;
        
        private ActorSystem _actorSystem;
        private ClusterClientReceptionist _receptionist;
        
        public ClusterNode(int nodeId, int port)
        {
            _nodeId = nodeId;
            _port = port;
        }

        public async Task StartAsync()
        {
            var setup = ActorSystemSetup.Create(
                BootstrapSetup.Create()
                    .WithConfig(Util.Config(_port))
                    .WithConfigFallback(DistributedPubSub.DefaultConfig())
                    .WithConfigFallback(ClusterClientReceptionist.DefaultConfig())
                    .WithActorRefProvider(ProviderSelection.Cluster.Instance));
            
            _actorSystem = ActorSystem.Create(Consts.NodeName, setup);
            var cluster = Akka.Cluster.Cluster.Get(_actorSystem);
            await cluster.JoinSeedNodesAsync(Consts.Seeds);

            _receptionist = ClusterClientReceptionist.Get(_actorSystem);
            foreach (var id in Enumerable.Range(_nodeId * Consts.ActorCount, Consts.ActorCount))
            {
                var actor = _actorSystem.ActorOf(Props.Create(() => new ServiceActor(id)), Util.ActorName(id));
                _receptionist.RegisterService(actor);
            }
        }

        public async Task StopAsync()
        {
            await _actorSystem.Terminate();
        }
    }
}