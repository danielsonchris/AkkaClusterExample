using System;

namespace AkkaClusterExample
{
    class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();

            Client client = new Client();
            client.Start();

            string input = null;
            while (input != "e")
            {
                //if (!client.IsRunning && input != "c")
                //{
                //    Console.WriteLine("Press 'c' and <enter> to start the client.");
                //} 
                //else if (input == "c")
                //{
                //    client.Start();
                //}
                Console.WriteLine("Press 'e' and <enter> to exit.");
                input = Console.ReadLine();
            }

            client.Stop();
            server.Stop();
        }
    }
}
