//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2021 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2021 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using ChatMessages;

namespace ChatServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
akka {  
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 8081
            hostname = 0.0.0.0
            public-hostname = localhost
        }
    }
}
");

            var setup = BootstrapSetup.Create().WithConfig(config);
            using var system = ActorSystem.Create("MyServer", setup);
            system.ActorOf(Props.Create(() => new ChatServerActor()), "ChatServer");

            Console.ReadLine();

            await system.Terminate();
        }
    }

    class ChatServerActor : ReceiveActor, ILogReceive
    {
        private readonly HashSet<IActorRef> _clients = new ();

        public ChatServerActor()
        {
            Receive<SayRequest>(message =>
            {
                var response = new SayResponse
                {
                    Username = message.Username,
                    Text = message.Text,
                };
                foreach (var client in _clients) client.Tell(response, Self);
            });

            Receive<ConnectRequest>(_ =>
            {
                _clients.Add(Sender);
                Sender.Tell(new ConnectResponse
                {
                    Message = "Hello and welcome to Akka.NET chat example",
                }, Self);
            });

            Receive<NickRequest>(message =>
            {
                var response = new NickResponse
                {
                    OldUsername = message.OldUsername,
                    NewUsername = message.NewUsername,
                };

                foreach (var client in _clients) client.Tell(response, Self);
            });
        }
    }
}

