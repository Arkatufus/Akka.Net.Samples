using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Simple.Cluster
{
    class Program
    {
        private static readonly string[] Ports = {"25251", "25252", "0"};

        internal static async Task<int> Main(string[] args)
        {
            var systems = new List<ActorSystem>();
            foreach (var port in Ports)
            {
                systems.Add(Startup(port));
            }

            Console.ReadLine();

            var tasks = systems.Select(sys => sys.Terminate());
            await Task.WhenAll(tasks);

            return 0;
        }

        private static ActorSystem Startup(string port)
        {
            var config = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.port = {port}")
                .WithFallback(ConfigurationFactory.ParseString(File.ReadAllText("application.conf")));

            var setup = BootstrapSetup.Create()
                .WithConfig(config)
                .WithActorRefProvider(ProviderSelection.Cluster.Instance);

            var system = ActorSystem.Create("ClusterSystem", setup);

            system.ActorOf(ClusterListener.Props(), "ClusterListener");
            return system;
        }
    }
}
