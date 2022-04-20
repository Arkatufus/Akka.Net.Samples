//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2021 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2021 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Sharding;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Akka.Persistence.Sqlite;
using Akka.Util;

namespace ClusterSharding.Node
{
    using ClusterSharding = Akka.Cluster.Sharding.ClusterSharding;

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(await File.ReadAllTextAsync("app.conf"))
                .WithFallback(ClusterSingletonManager.DefaultConfig());

            using (var system = ActorSystem.Create("sharded-cluster-system", config))
            {
                var cluster = Cluster.Get(system);
                var persistence = SqlitePersistence.Get(system);
                await RunExample(system);
                Console.ReadLine();
            }
        }

        private static async Task RunExample(ActorSystem system)
        {
            var sharding = ClusterSharding.Get(system);
            var shardRegion = await sharding.StartAsync(
                typeName: "customer",
                entityProps: Props.Create<Customer>(),
                settings: ClusterShardingSettings.Create(system),
                messageExtractor: new MessageExtractor(10));

            await Task.Delay(5000);
            Console.Write("Press ENTER to start producing messages...");
            Console.ReadLine();

            ProduceMessages(system, shardRegion);
        }

        private static void ProduceMessages(ActorSystem system, IActorRef shardRegion)
        {
            var customers = new[] { "Yoda", "Obi-Wan", "Darth Vader", "Princess Leia", "Luke Skywalker", "R2D2", "Han Solo", "Chewbacca", "Jabba" };
            var items = new[] { "Yoghurt", "Fruits", "Lightsaber", "Fluffy toy", "Dreamcatcher", "Candies", "Cigars", "Chicken nuggets", "French fries" };

            system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(3), () =>
            {
                var customer = PickRandom(customers);
                var item = PickRandom(items);
                var message = new ShardEnvelope(customer, new Customer.PurchaseItem(item));

                shardRegion.Tell(message);
            });
        }

        private static T PickRandom<T>(T[] items) => items[ThreadLocalRandom.Current.Next(items.Length)];
    }
}
