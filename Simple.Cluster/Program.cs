using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Cluster;

namespace Simple.Cluster
{
    class Program
    {
        private static string[] _ports = {"25251", "25252"};

        internal static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new[] {"25251", "25252", "0"};
            }

            var systems = new List<ActorSystem>();
            foreach (var arg in args)
            {
                systems.Add(Startup(arg));
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
