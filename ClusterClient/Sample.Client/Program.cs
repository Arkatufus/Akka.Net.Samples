using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Cluster.Tools.Client;
using Sample.Shared;

namespace Sample.Client
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            
            #region Console shutdown setup
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };
        
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                cts.Cancel();
            };
            #endregion

            var setup = ActorSystemSetup.Create(
                BootstrapSetup.Create()
                    .WithConfig(Util.Config(Consts.PortStart - 1))
                    .WithActorRefProvider(ProviderSelection.Remote.Instance));
            
            var system = ActorSystem.Create("ClusterClientSystem", setup);
            var clientActor = system.ActorOf(Props.Create(() => new ClientActor()));
            
            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (TaskCanceledException _)
            {
                // expected
            }

            await system.Terminate();
        }
    }
}