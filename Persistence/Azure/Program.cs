using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence;
using Akka.Persistence.Azure;
using Akka.Persistence.Azure.Snapshot;
using Azure.Identity;

namespace Azure
{
    class Program
    {
        // INSERT AZURE ACCOUNT NAME HERE
        private const string YourAccountName = "INSERT AZURE ACCOUNT NAME HERE"; 

        static async Task Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
akka.persistence.snapshot-store {
    plugin = ""akka.persistence.snapshot-store.azure-blob-store""
    azure-blob-store {
        container-name = snapshot-greg
        connection-string = not_used
    }
}")
                .WithFallback(AzurePersistence.DefaultConfig);

            var bootstrap = BootstrapSetup.Create()
                .WithConfig(config)
                .And(AzureBlobSnapshotSetup.Create(
                    new Uri($"https://{YourAccountName}.blob.core.windows.net"),
                    new DefaultAzureCredential()));

            var actorSystem = ActorSystem.Create("AzureTest", bootstrap);
            var actor = actorSystem.ActorOf(Props.Create<TestActor>());

            // let everything synched up
            await Task.Delay(1000);

            var response = await actor.Ask<StateResponse>(GetState.Instance);

            switch (response.State)
            {
                case "three":
                    Console.WriteLine($"Actor recovered with state: [{response.State}]");
                    break;
                case "undefined":
                    actor.Tell(new SetState("one"));
                    actor.Tell(new SetState("two"));
                    actor.Tell(new SetState("three"));
                    actor.Tell(Snap.Instance);

                    response = await actor.Ask<StateResponse>(GetState.Instance);
                    Console.WriteLine($"Last actor state: [{response.State}]");
                    break;
                case var unknown:
                    Console.WriteLine($"Unknown actor state [{unknown}]");
                    break;
            }

            await actorSystem.Terminate();
        }
    }

    public class TestActor : PersistentActor
    {
        private string _state = "undefined";

        protected override bool ReceiveRecover(object message)
        {
            switch (message)
            {
                case SnapshotOffer snapshot:
                    _state = (string)snapshot.Snapshot;
                    return true;
                case RecoveryCompleted _:
                    return true;
            }

            return false;
        }

        protected override bool ReceiveCommand(object message)
        {
            switch (message)
            {
                case SetState msg:
                    Persist(msg, evt =>
                    {
                        _state = evt.State;
                    });
                    return true;
                case GetState _:
                    Sender.Tell(new StateResponse(_state));
                    return true;
                case Snap _:
                    SaveSnapshot(_state);
                    return true;
                default:
                    return false;
            }
        }

        public override string PersistenceId { get; } = "TestActorWithState";
    }

    public class Snap
    {
        public static Snap Instance { get; } = new Snap();
        private Snap() { }
    }

    public class GetState
    {
        public static GetState Instance { get; } = new GetState();
        private GetState() { }
    }

    public class SetState
    {
        public SetState(string state)
        {
            State = state;
        }

        public string State { get; }
    }

    public class StateResponse
    {
        public StateResponse(string state)
        {
            State = state;
        }

        public string State { get; }
    }

}
