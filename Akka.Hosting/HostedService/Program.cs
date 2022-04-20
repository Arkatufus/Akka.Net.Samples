using Akka.Actor;
using Akka.Hosting;
using HostedService;
using HostedService.Actors;

#region Console shutdown setup

var running = true;
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    running = false;
};
AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    running = false;
};
            
#endregion

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddAkka("MyAkkaSystem", (builder, serviceProvider) =>
        {
            builder.WithActors((actorSystem, actorRegistry) =>
            {
                var supervisor = actorSystem.ActorOf(SupervisingActor.Props(), "supervisor");
                actorRegistry.TryRegister<SupervisingActor>(supervisor); // Register for DI
            });
        });
    })
    .Build();

await host.StartAsync();

var registry = host.Services.GetRequiredService<ActorRegistry>();
var supervisorActor = registry.Get<SupervisingActor>();
var rnd = new Random();

while (running)
{
    var message = new Message(rnd.Next(0, 20).ToString(), rnd.NextDouble());
    
    Console.WriteLine($">>>>>>> Sending {message.Payload} to Id {message.Id}");
    var response = await supervisorActor.Ask<double>(message);
    Console.WriteLine($"<<<<<<< Received {response}");

    await Task.Delay(1000);
}

await host.StopAsync();