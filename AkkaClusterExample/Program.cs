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
                Console.WriteLine("Press 'e' and <enter> to exit.");
                input = Console.ReadLine();
            }

            client.Stop();
            server.Stop();
        }
    }
}
