using System;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Persistence;
using static SharedLibrary.Protocol;

namespace ShardingNode
{

    public class CounterPersistentActor : PersistentActor
    {
        private int _count;
        
        public CounterPersistentActor()
        {
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(120));
        }
        
        public override string PersistenceId => $"Counter-{Self.Path.Name}";
        
        private void UpdateState(CounterChanged @event)
        {
            _count += @event.Delta;
        }
        
        protected override bool ReceiveRecover(object message)
        {
            switch (message)
            {
                case CounterChanged evt:
                    UpdateState(evt);
                    return true;
                default:
                    return false;
            }
        }

        protected override bool ReceiveCommand(object message)
        {
            switch (message)
            {
                case Increment _:
                    Persist(new CounterChanged(1), UpdateState);
                    return true;
                case Decrement _:
                    Persist(new CounterChanged(-1), UpdateState);
                    return true;
                case Get _:
                    Sender.Tell(_count, Self);
                    return true;
                case ReceiveTimeout _:
                    Context.Parent.Tell(new Passivate(Stop.Instance));
                    return true;
                case Stop _:
                    Context.Stop(Self);
                    return true;
                default:
                    return false;
            }
        }
    }
}