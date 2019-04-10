using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;

namespace AkkaClusterExample
{
    /// <summary>
    /// The Ping class, which implements the IConsistenHashable interface.
    /// </summary>
    public class Ping : IConsistentHashable
    {
        public string Message { private set; get; }

        public object ConsistentHashKey => Guid.NewGuid();

        public Ping(string message)
        {
            Message = message;
        }
    }

    public class Pong
    {
        public string Message { private set; get; }

        public Pong(string message)
        {
            Message = message;
        }
    }

    public class GreetingActor : ReceiveActor
    {
        public GreetingActor()
        {
            Console.WriteLine(Context.Self + ": GreetingActor init ...");

            //var testing = Context.ActorOf(Props.Create<EchoActor>().WithRouter(Context.Props.RouterConfig));
            var testing = Context.ActorOf(Props.Create<EchoActor>());

            //Receive<Ping>(greet =>
            //{
            //    Console.WriteLine(Context.Self + " ==> " + greet.Message);

            //    // var testing = Context.ActorOf(Props.Create<EchoActor>().WithDeploy(new Deploy(
            //    //    new RemoteScope(Address.Parse("akka.tcp://ClusterSystem@127.0.0.1:2551"))
            //    // )));
            //    testing.Tell(new Hello(greet.Message));
            //    //Sender.Tell(new Pong("Hi, I received <" + greet.Message + "> ^_^ "));
            //});

            ReceiveAsync<Ping>(async ping =>
            {


                await Task.Delay(0);
                Console.WriteLine("Async! " + ping.Message + " " + Context.Self.Path);
            });

            Receive<Pong>(greet => { Console.WriteLine(Context.Self + " ==> " + greet.Message); });
        }

        public override void AroundPreStart()
        {
            base.AroundPreStart();
            Console.WriteLine("AroundPreStart called");
        }

    }

    public class EchoActor : ReceiveActor
    {
        public EchoActor()
        {
            Receive<Hello>(hello =>
            {
                Console.WriteLine("Sender:{0}; Self: {1}: FROM ECHO ACTOR {2}", Sender, Context.Self, hello.Message);
                Sender.Tell(hello);
            });
        }
    }

    public class Hello
    {
        public Hello(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }

    public class SomeActor : UntypedActor
    {
        public SomeActor(string someArg, long otherArg)
        {
            Console.WriteLine("Constructing SomeActor with {0},{1}", someArg, otherArg);
        }

        protected override void OnReceive(object message)
        {
            if (message is long)
            {
                Console.Write(".");
            }
            else
            {
                Console.WriteLine("{0} got {1}", Self.Path.ToStringWithAddress(), message);
                Sender.Tell("hello");
            }
        }
    }
}
