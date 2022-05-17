using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;

namespace Sample.Shared
{
    public static class Consts
    {
        public const int NodeCount = 5;
        public const int PortStart = 15000;
        public const string NodeName = "ClusterNode";
        public const string ActorPrefix = "Service-";
        public const int ActorCount = 2;
        public const int TotalActors = NodeCount * ActorCount;

        private static Address[] _seeds;
        public static Address[] Seeds
        {
            get
            {
                if (_seeds != null)
                    return _seeds;

                _seeds = Enumerable.Range(0, NodeCount)
                    .Select(id => Address.Parse($"akka.tcp://{NodeName}@localhost:{PortStart + id}"))
                    .ToArray();
                return _seeds;
            }
        }

        private static ImmutableHashSet<ActorPath> _contactPoints;
        public static ImmutableHashSet<ActorPath> ContactPoints
        {
            get
            {
                if (_contactPoints != null)
                    return _contactPoints;

                _contactPoints = Seeds
                    .Select(seed => ActorPath.Parse(seed.ToString()) / "system" / "receptionist")
                    .ToImmutableHashSet();
                return _contactPoints;
            }
        } 
    }
}