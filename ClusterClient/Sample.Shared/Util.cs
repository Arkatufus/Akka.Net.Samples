using System.Linq;
using Akka.Configuration;

namespace Sample.Shared
{
    public static class Util
    {
        public static string ActorName(int id) => Consts.ActorPrefix + id;
        public static string ActorPath(int id) => "/user/" + ActorName(id);
        public static Config Config(int port) => ConfigurationFactory.ParseString($@"
akka {{
    remote.dot-netty.tcp.port = {port}
    remote.dot-netty.tcp.public-hostname = localhost
}}");

    }
}