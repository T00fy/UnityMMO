using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace MMOServer
{


    public class LoginServer
    {
        public static List<ClientConnection> mConnectionList = new List<ClientConnection>();
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public const int BUFFER_SIZE = 65535;
        private Socket listener;

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".

            // Create a TCP/IP socket.
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.IPv6Any, 3425);
            listener = new Socket(serverEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.DualMode = true;

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(serverEndPoint);
                listener.Listen(100);

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

        private void AcceptCallback(IAsyncResult ar)
        {
            ClientConnection client = null;
            

            try
            {
                Socket s = (Socket)ar.AsyncState;
                client = new ClientConnection()
                {
                    PacketProcessor = new PacketProcessor(),
                    socket = s.EndAccept(ar),
                    buffer = new byte[BUFFER_SIZE]
                };
                lock (mConnectionList)
                {
                    mConnectionList.Add(client);
                }
                //queue up incoming receive data connection
                client.socket.BeginReceive(client.buffer, 0, client.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), client);
                //start accepting connections again
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                client.fullAddress = string.Format("{0}:{1}", (client.socket.RemoteEndPoint as IPEndPoint).Address, (client.socket.RemoteEndPoint as IPEndPoint).Port);
                Console.WriteLine(client.GetFullAddress());
            }


            catch (Exception)
            {
                if (client.socket == null)
                {
                    client.socket.Close();
                    lock (mConnectionList)
                    {
                        mConnectionList.Remove(client);
                    }
                }
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }


        }

        private void ReceiveCallBack(IAsyncResult ar)
        {

            //need to setup buffer of length PacketController 
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            ClientConnection client = (ClientConnection)ar.AsyncState;
            try
            {
                
                int bytesRead = client.socket.EndReceive(ar);
                Console.WriteLine("bytes read" + bytesRead);
                //allows to pause traffic and restart for debugging purposes.
                bytesRead += client.lastPartialSize;
                if (bytesRead > 0)
                {
                    int offset = 0;

                    //build/compile packets until can no longer or data is finished
                    while (client.socket.Connected)
                    {
                        BasePacket basePacket = BuildPacket(ref offset, client.buffer, bytesRead);
                        if (basePacket == null)
                        {
                            break;
                        }
                        else
                        {
                            
                            client.PacketProcessor.ProcessPacket(client, basePacket);
                        }

                    }
                    //Not all bytes consumed, transfer leftover to beginning
                    if (offset < bytesRead)
                    {
                        Array.Copy(client.buffer, offset, client.buffer, 0, bytesRead - offset);
                    }

                    Array.Clear(client.buffer, bytesRead - offset, client.buffer.Length - (bytesRead - offset));

                    //allows to pause traffic and restart for debugging purposes.
                    client.lastPartialSize = bytesRead - offset;

                    //Build any queued subpackets into basepackets and send
                    client.FlushQueuedSendPackets();

                    if (offset < bytesRead)
                    //need offset since not all bytes consumed
                    {
                        client.socket.BeginReceive(client.buffer, bytesRead - offset, client.buffer.Length - (bytesRead - offset), SocketFlags.None, new AsyncCallback(ReceiveCallBack), client);
                    }
                    else
                    {
                        client.socket.BeginReceive(client.buffer, 0, client.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), client);
                    }
                }
                else
                {
                    Console.WriteLine("Client at {0} has disconnected", client.GetFullAddress());

                    lock (mConnectionList)
                    {
                        client.Disconnect();
                        mConnectionList.Remove(client);
                    }
                }


            }
            catch (SocketException)
            {
                if (client.socket != null)
                {
                    Console.WriteLine("Client at {0} has disconnected", client.GetFullAddress());

                    lock (mConnectionList)
                    {
                        mConnectionList.Remove(client);
                    }
                }
            }


        }

        /// <summary>
        /// Builds a packet from the incoming buffer + offset. If a packet can be built, it is returned else null.
        /// </summary>
        /// <param name="offset">Current offset in buffer.</param>
        /// <param name="buffer">Incoming buffer.</param>
        /// <returns>Returns either a BasePacket or null if not enough data.</returns>
        public BasePacket BuildPacket(ref int offset, byte[] buffer, int bytesRead)
        {
            BasePacket newPacket = null;

            //Too small to even get length
            if (bytesRead <= offset)
                return null;

            ushort packetSize = BitConverter.ToUInt16(buffer, offset);

            //Too small to whole packet
            if (bytesRead < offset + packetSize)
                return null;

            if (buffer.Length < offset + packetSize)
                return null;

            try
            {
                
                newPacket = new BasePacket(buffer, ref offset);
            }
            catch (OverflowException e)
            {

                Console.WriteLine(e.ToString());
                return null;
            }

            return newPacket;
        }
    }
}