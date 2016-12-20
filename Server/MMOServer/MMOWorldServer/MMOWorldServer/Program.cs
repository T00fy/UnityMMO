using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMOServer;
using System.Threading;

namespace MMOWorldServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-----MMO World Server-----");
            Console.WriteLine("Checking DB connection");
            Database db = new Database();
            string databaseStatus = db.CheckDbConnection();
            if (databaseStatus == "OK")
            {
                Console.WriteLine("Connected to DB.");
                WorldServer server = new WorldServer();
                bool startServer = server.StartServer();
                while (startServer)
                {
                    string input = Console.ReadLine();
                    //TODO: allow for commands to be entered into the server here
                }
                //Thread.Sleep(10000);
            }
            else
            {
                Console.WriteLine("FAILED. Could not connect to MYSQL DB.");
                Console.WriteLine("Please press any key to continue...");
                Console.ReadKey();
            }

             
        }
    }
}
