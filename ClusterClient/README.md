# ClusterClient and ClusterClientReceptionist Example

In this example:
1. __Sample.Shared__: Codes shared between the client and cluster project
2. __Sample.Cluster__: This project will spin off (by default) a 5 node Akka.NET cluster with 2 registered actors in each nodes. 
  Each node will expose a `ClusterClientReceptionist` that can be used to access the registered actors from outside the cluster.
3. __Sample.Client__: This project will start an `ActorSystem` with a `ClusterClient` that will connect to the `ClusterClientReceptionist`
  inside the cluster and starts sending and receiving messages periodically to and from the cluster.

To run this example, open two consoles. In one console, go to the _Sample.Cluster_ directory and in the other, go to the _Sample.Client_ directory.
Run `dotnet.run` on each console and observe that the two processes are communicating with each other.
