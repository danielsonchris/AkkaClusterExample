# AkkaClusterExample

A simple console example on how to use Akka.Net as a cluster.  Coded strictly using .Net Core.

The goal was to implement build a simple cluster with only the following nuget packages.
```xml
<PackageReference Include="Akka" Version="1.3.12" />
<PackageReference Include="Akka.Remote" Version="1.3.12" />
<PackageReference Include="Akka.Cluster" Version="1.3.12" />
```

The process loads the 2 nodes on the server-side using a specified role of clusterbackend.  The client then loads using a role of "client" and immediately starts a router

```cs
ClusterRouterGroup crg = new ClusterRouterGroup(
                            local: new ConsistentHashingGroup(paths),
                            settings: new ClusterRouterGroupSettings(
                                10,
                                ImmutableHashSet.Create(paths.ToArray()),
                                allowLocalRoutees: false,
                                useRole: null));
var greetingRouter = system.ActorOf(crg.Props(), "router");
```

The cluster then jump starts and begins processing.
