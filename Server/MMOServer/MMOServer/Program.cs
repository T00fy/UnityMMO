using System;
using System.Threading;


namespace MMOServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Setting up server...");
            Console.WriteLine("Checking DB connection");
            Database db = new Database();
            string connStatus = db.CheckDbConnection();
            if (connStatus == "OK")
            {
                Console.WriteLine("Connected to DB.");
                LoginServer server = new LoginServer();
                server.StartListening();
                while (true) Thread.Sleep(10000);
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
