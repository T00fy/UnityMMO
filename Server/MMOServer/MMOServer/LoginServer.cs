using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CryptSharp;
using System.Threading;

namespace MMOServer
{
    /*   class LoginServer
       {
           private static byte[] buffer = new byte[1024];
           private static List<Socket> clientSockets = new List<Socket>();
           private static Socket serverSocket = new Socket
               (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


           static void Main(string[] args)
           {
               SetupServer();
               Console.ReadLine(); //keeps the console alive
           }

           //
           //Start to accept any connections
           //
           private static void SetupServer()
           {
               Console.WriteLine("Setting up server...");
               serverSocket.Bind(new IPEndPoint(IPAddress.Any, 3425));
               serverSocket.Listen(500);
               serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);


           }


           //
           //Asynchronously accept an incoming connection and start to receive data
           //
           private static void AcceptCallBack(IAsyncResult aSyncResult)
           {
               Socket socket = serverSocket.EndAccept(aSyncResult); //creates a new socket if the result succeeded that handles the remote connection
               clientSockets.Add(socket);
               Console.WriteLine("Client connected");
               socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
               serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null); //allows for more than one connection
           }


           //
           //Finish accepting all the data and push it back into the buffer array
           //
           private static void ReceiveCallBack(IAsyncResult aSyncResult)
           {
               Socket socket = (Socket)aSyncResult.AsyncState; //the callback socket
               int received = socket.EndReceive(aSyncResult); //number of bytes received
               byte[] dataBuf = new byte[received];
               Array.Copy(buffer, dataBuf, received); //copy from buffer to databuf array


               string text = Encoding.ASCII.GetString(dataBuf);
               string[] cmdList = text.Split(' ');
               //TODO:
               //check if it's a login request or a registration request
               if (cmdList[0].Equals("register")){
                   Console.WriteLine("Received register request from user: " + cmdList[1] + " pwd: " + cmdList[2]);
                   //query database and add user credentials
                   //don't allow spaces in password or username (done through client)
               }
               //login
               if (cmdList[0].Equals("login"))
               {
                   //check user name and hashed password against mysql 
                   //if exists authentication is successful
                   //send success message
                   //connect client to character server
                       //pass player id, connection and server password to character server
               }

               //make login screen for client
               //add registration button
               //this will send command "register user;encryptedpw"
               //don't allow ; in username
               //send encrypted username/pw to server 
               //authenticate and change scene to character select screen

               //registration class (maybe through apache?)

               byte[] dataToSend = Encoding.ASCII.GetBytes("SUCCESS");




               socket.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket); //send data to client
     //          socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
               //begin receiving again


           }



           //
           //Finish sending data to client
           //
           private static void SendCallBack(IAsyncResult aSyncResult)
           {
               Socket socket = (Socket)aSyncResult.AsyncState; //the callback socket
               socket.EndSend(aSyncResult);
           }
       }
   }*/
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

        public LoginServer()
        {
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

            Console.WriteLine("Client connected.");
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
    new AsyncCallback(ReceiveCallBack), state);
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

                        if (cmdList[0] == "register")
                        {
                            Console.WriteLine("Received register request from user: " + cmdList[1] + " pwd: " + cmdList[2]);
                            Send(handler, "SUCCESS");
                        }
                        if (cmdList[0] == "login")
                        {
                            Send(handler, "SUCCESS");
                        }


                    }  
                }
                // Not all data received. Get more.
           //     else {
          //          handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
            catch (Exception e) {
                Console.WriteLine(e.ToString());
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


        public static void Main(String[] args)
        {
            Console.WriteLine("Setting up server....");
            StartListening();
        }
    }
}