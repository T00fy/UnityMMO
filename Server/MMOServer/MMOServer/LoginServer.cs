using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CryptSharp;

namespace MMOServer
{
    class LoginServer
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
            serverSocket.Listen(5);
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
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
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
}
