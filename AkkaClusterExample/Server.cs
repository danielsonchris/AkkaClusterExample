using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace AkkaClusterExample
{
    public class Server
    {
        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            Task.Run(() =>
            {
                //_RunRemote();
                _RunCluster();
            });
        }

        /// <summary>
        /// Runs the cluster test.
        /// </summary>
        private void _RunCluster()
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                    }
                    remote {
                        log-remote-lifecycle-events = DEBUG
                        dot-netty.tcp {
                            hostname = ""127.0.0.1""
                            port = 0
                        }
                    }
                    cluster {
                        seed-nodes = [
                            ""akka.tcp://ClusterServer@127.0.0.1:2551"",
                            ""akka.tcp://ClusterServer@127.0.0.1:2552"",
                        ]
                        auto-down-unreachable-after = 30s
                        roles = [clusterbackend]
                    }
                }");
            List<ActorSystem> systems = new List<ActorSystem>();
            List<int> ports = new List<int>() { 2551, 2552 };
            foreach (var port in ports)
            {
                var newConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.port={port}")
                    .WithFallback(config);
                var system = ActorSystem.Create("ClusterServer", newConfig);
                system.ActorOf(Props.Create<GreetingActor>(), "greeting");
                system.ActorOf(Props.Create<EchoActor>(), "echo");

                systems.Add(system);
            }
            while (IsRunning)
            {
                Thread.Sleep(1000);
            }
            foreach (var system in systems)
            {
                system.Dispose();
            }
        }

        /// <summary>
        /// Runs the remote only test.
        /// </summary>
        private void _RunRemote()
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = remote
                    }
                    remote {
                        log-remote-lifecycle-events = DEBUG
                        dot-netty.tcp {
                            hostname = ""127.0.0.1""
                            port = 0
                        }
                    }
                    roles = [scheduler]
                }");
            List<ActorSystem> systems = new List<ActorSystem>();
            List<int> ports = new List<int>() { 2551, 2552 };
            foreach (var port in ports)
            {
                var newConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.port={port}")
                    .WithFallback(config);
                var system = ActorSystem.Create("ClusterServer", newConfig);
                system.ActorOf(Props.Create<GreetingActor>(), "greeting");
                system.ActorOf(Props.Create<EchoActor>(), "echo");

                systems.Add(system);
            }
            while (IsRunning)
            {
                Thread.Sleep(1000);
            }
            foreach (var system in systems)
            {
                system.Dispose();
            }
        }

        private bool IsRunning { get; set; } = false;

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
