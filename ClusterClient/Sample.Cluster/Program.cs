using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sample.Shared;

namespace Sample.Cluster
{
    public static class Program
    {
        private static readonly ClusterNode[] Nodes = new ClusterNode[Consts.NodeCount];
        
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
        
            foreach (var id in Enumerable.Range(0, Consts.NodeCount))
            {
                Nodes[id] = new ClusterNode(id, Consts.PortStart + id);
            }

            try
            {
                await Task.WhenAll(Nodes.Select(node => node.StartAsync()));
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (TaskCanceledException _)
            {
                // expected
            }

            await Task.WhenAll(Nodes.Select(node => node.StopAsync()));
        }
    }
}