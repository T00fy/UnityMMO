using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CryptSharp;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace MMOServer
{
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
        

    public class LoginServer {
        private static List<Socket> clientSockets = new List<Socket>();
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static MySqlConnection conn;

        public static void Main(String[] args)
        {
            Console.WriteLine("Setting up server...");
            ConnectToDb();
            StartListening();
        }




        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(new IPEndPoint(IPAddress.Any, 3425));
                listener.Listen(500);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            clientSockets.Add(handler);
            Console.WriteLine("Client connected.");
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);


        }

        public static void ReceiveCallBack(IAsyncResult ar)
        {

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);
            try
            {
                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read 
                    // more data.
                    var cmdList = state.sb.ToString().Split(' ');
                    CommandResponse(handler, cmdList);
                }
                else {
                    //client has sent 0 bytes shutdown ack
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
                    handler.Close();
                }  
           }
                // Not all data received. Get more.
                //to be implemented later if bugs found
                    
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private static void CommandResponse(Socket handler, string[] cmdList)
        {
            if (cmdList[0] == "register")
            {
                Console.WriteLine("Received register request from user: " + cmdList[1] + " pwd: " + cmdList[2]);
                var succeeded = AddUserToDb(cmdList[1], cmdList[2]);
                if (succeeded)
                {
                    Send(handler, "SUCCESS");
                }
                else
                {
                    Send(handler, "FAILED");

                }

            }
            if (cmdList[0] == "login")
            { 
                Console.WriteLine("Received Login request for: " + cmdList[1] + " pwd: " + cmdList[2]);
                ArrayList list = CheckUserInDb(cmdList[1], cmdList[2]);
                if (list != null) {
                    if (list.Count == 2)
                    {
                        //try and open connection to character server
                        Send(handler, "Login Successful");
                  //      handler.DuplicateAndClose()
                        //connect and send socket to Char server here

                    }
                    if (list.Count == 1)
                    {
                        Send(handler, "Incorrect password");
                    }
                    if (list.Count == 0)
                    {
                        Send(handler, "Account not found");
                    }

                }
                else
                {
                    Send(handler, "Account not found");
                }
                
            }

        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }




        /*
         *
         *
         *DB METHODS
         *
         *
         *  
        */

        private static bool AddUserToDb(string userName, string password)
        {
            try
            {
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "INSERT INTO account(username, password) VALUES(@user, @pass)";
                command.Parameters.AddWithValue("@user", userName);
                command.Parameters.AddWithValue("@pass", password);
                command.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Duplicate username attempted to be regsitered");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;

            }

        }

        private static ArrayList CheckUserInDb(string userName, string password)
        {
            ArrayList list = new ArrayList();
            try
            {
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "SELECT `username`, `password` FROM `account` WHERE `username`=@user";
                command.Parameters.AddWithValue("@user", userName);
                MySqlDataReader rdr = command.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        list.Add(rdr.GetString(0));
                        list.Add(rdr.GetString(1));
                    }
                }
                rdr.Close();
                return list;

            }
            catch (MySqlException)
            {
                Console.WriteLine("MySQL error");
                return list;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return list;

            }
        }


        private static void ConnectToDb()
        {
            var connection = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();
            conn = new MySqlConnection(connection);
            try
            {
                Console.WriteLine("Connecting to MYSQL server...");
                conn.Open();
                Console.WriteLine("Connected to DB");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }
            
        }
    }
}