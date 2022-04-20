using System;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using static SharedLibrary.Protocol;

namespace ShardingNode
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("How to use: ShardingNode [port]");
                return;
            }

            if (!int.TryParse(args[0], out var port))
            {
                Console.WriteLine("How to use: ShardingNode [port]");
                return;
            }

            var config = ConfigurationFactory.ParseString($@"
akka.remote.dot-netty.tcp.hostname = localhost
akka.remote.dot-netty.tcp.port = {port}")
                .WithFallback(ClusterSharding.DefaultConfig());
            
            var system = ActorSystem.Create("ClusterSharding");

            var counterRegion = ClusterSharding.Get(system).Start(
                typeName: "Counter",
                entityProps: Props.Create<CounterPersistentActor>(),
                settings: ClusterShardingSettings.Create(system), 
                extractEntityId: ExtractEntityId,
                extractShardId: ExtractShardId);
            
        }
    }
}