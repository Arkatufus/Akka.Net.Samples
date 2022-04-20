using System;
using Akka.Cluster.Sharding;
using Akka.Util;

namespace SharedLibrary
{
    public static class Protocol
    {
        public sealed class Increment
        {
            public static Increment Instance => new Increment();
            private Increment() { }
        }

        public sealed class Decrement
        {
            public static Decrement Instance => new Decrement();
            private Decrement() { }
        }

        public sealed class Get
        {
            public Get(long counterId)
            {
                CounterId = counterId;
            }

            public long CounterId { get; }
        }

        public sealed class EntityEnvelope
        {
            public EntityEnvelope(long id, object payload)
            {
                Id = id;
                Payload = payload;
            }

            public long Id { get; }
            public object Payload { get; }
        }
        
        public sealed class Stop
        {
            public static Stop Instance => new Stop();
            private Stop() { }
        }

        public sealed class CounterChanged
        {
            public CounterChanged(int delta)
            {
                Delta = delta;
            }

            public int Delta { get; }
        }

        public const int NumberOfShard = 100;
        
        public static Option<(string, object)> ExtractEntityId(object message)
        {
            return message switch
            {
                EntityEnvelope env => (env.Id.ToString(), env.Payload),
                Get get => (get.CounterId.ToString(), get),
                _ => Option<(string, object)>.None
            };
        }
        
        public static string ExtractShardId(object message)
        {
            return message switch
            {
                EntityEnvelope env => (env.Id % NumberOfShard).ToString(),
                Get get => (get.CounterId % NumberOfShard).ToString(),
                ShardRegion.StartEntity msg => 
                    // StartEntity is used by remembering entities feature
                    (long.Parse(msg.EntityId) % NumberOfShard).ToString(),
                _ => throw new ArgumentException($"Wrong type [{message.GetType()}]", nameof(message))
            };
        }
    }
}