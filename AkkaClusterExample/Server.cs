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
                        roles = [scheduler]
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
        /// 
        /// </summary>
        private void _RunRemote()
        {
            //var config = ConfigurationFactory.ParseString(@"
            //akka {
            //    actor {
            //        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
            //        deployment {
            //            /greeting {
            //                router = round-robin-pool # routing strategy
            //                nr-of-instances = 1 # max number of total routees
            //                cluster {
            //                    enabled = on
            //                    allow-local-routees = off
            //                    use-role = server
            //                    max-nr-of-instances-per-node = 10
            //                }
            //            }
            //            /testing {
            //                router = round-robin-pool # routing strategy
            //                nr-of-instances = 1 # max number of total routees
            //                cluster {
            //                    enabled = on
            //                    allow-local-routees = off
            //                    use-role = server
            //                    max-nr-of-instances-per-node = 2
            //                }
            //            }
            //        }
            //        debug {  
            //          receive = on 
            //          autoreceive = on
            //          lifecycle = on
            //          event-stream = on
            //          unhandled = on
            //        }
            //    }
            //    remote {
            //        log-remote-lifecycle-events = DEBUG
            //        dot-netty.tcp {
            //            hostname = ""127.0.0.1""
            //            port = 2551
            //        }
            //    }
            //    cluster {
            //        seed-nodes = [
            //            ""akka.tcp://ClusterSystem@127.0.0.1:2551"",
            //        ]
            //        auto-down-unreachable-after = 30s
            //        roles = []
            //    }
            //}");

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
