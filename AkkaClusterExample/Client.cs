﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Routing;
using Akka.Configuration;
using Akka.Event;
using Akka.Routing;

namespace AkkaClusterExample
{
    /// <summary>
    /// Client class that demonstrates a simple Akka cluster client.
    /// The goal being to not use Lighthouse and avoid using the ClusterReceptionist.
    /// </summary>
    public class Client
    {

        private Task task = null; 
        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            task = Task.Run(() =>
            {
                //_Run();
                ClusterRun();
            });
        }

        /// <summary>
        /// Runs the cluster test.
        /// </summary>
        private void ClusterRun()
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                    }
                    remote {
                        log-remote-lifecycle-events = DEBUG
                        dot-netty.tcp {
                            #hostname = ""127.0.0.1""
                            port = 0
                        }
                    }
                    cluster {
                        seed-nodes = [
                            ""akka.tcp://ClusterServer@127.0.0.1:2551"",
                            ""akka.tcp://ClusterServer@127.0.0.1:2552""
                        ]
                        roles = [client]
                    }
                }");

            using (var system = ActorSystem.Create("ClusterServer", config))
            {
                var deadletterWatchMonitorProps = Props.Create(() => new DeadletterMonitor());
                var deadletterWatchActorRef = system.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");

                // subscribe to the event stream for messages of type "DeadLetter"
                system.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));

                var paths = new List<string> { "/user/greeting" };
                //ConsistentHashMapping hashMapping = msg =>
                //{
                //    if (msg is string) return msg;
                //    return null;
                //};
                //build a basic router
                var greetingRouter =
                    system.ActorOf(
                        new ClusterRouterGroup(
                            local: new ConsistentHashingGroup(paths),//.WithHashMapping(hashMapping),
                            settings: new ClusterRouterGroupSettings(
                                10,
                                ImmutableHashSet.Create(paths.ToArray()),
                                allowLocalRoutees: true,
                                useRole: null))
                            .Props(), "router");

                //var greetingRouter = system.ActorOf(
                    //Props.Empty.WithRouter(new ClusterRouterGroup(new ConsistentHashingGroup(greetingURI), router)));
                        //new ClusterRouterGroupSettings(10, false, "client", ImmutableHashSet.Create(greetingURI)))));

                int count = 0;
                while (IsRunning)
                {
                    //greetingRouter.Ask(new Ping("neato"));
                    greetingRouter.Tell(new Ping($"test {count++}"));

                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Run an example using strictly "remote" and no cluster technology.
        /// </summary>
        private void Run()
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
                            port = 8081
                        }
                    }
                    roles = []
                }");

            using (var system = ActorSystem.Create("Sniffy", config))
            {
                var greeting = system.ActorSelection("akka.tcp://ClusterServer@127.0.0.1:2551/user/greeting");
                //var echo = system.ActorSelection("akka.tcp://ClusterServer@127.0.0.1:2551/user/echo");
                var greetingActorRef = system.ActorOf(Props.Create<GreetingActor>());
                var echoActorRef = system.ActorOf(Props.Create<EchoActor>());

                int count = 0;
                while (IsRunning)
                {
                    greeting.Tell(new Ping($"test {count++}"), greetingActorRef);
                    Thread.Sleep(1000);
                }
            }
        }

        public bool IsRunning { get; set; } = false;

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
