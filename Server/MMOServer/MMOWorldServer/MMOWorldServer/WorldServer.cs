using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MMOServer;
using MMOWorldServer.Data;
using MMOWorldServer.Actors;
using System.Collections.Concurrent;

namespace MMOWorldServer
{
    class WorldServer
    {
        public const int BUFFER_SIZE = 0xFFFF; //Max basepacket size is 0xFFFF
        public const int BACKLOG = 100;
        public const int HEALTH_THREAD_SLEEP_TIME = 5;

        private static WorldServer mSelf;

        private Socket mServerSocket;

        //key is characterId
        public static ConcurrentDictionary<uint, Character> mConnectedPlayerList = new ConcurrentDictionary<uint, Character>();

        //raw connections
        private static List<WorldClientConnection> mConnectionList = new List<WorldClientConnection>();

        private static WorldManager mWorldManager;
        private static Dictionary<uint, Item> gamedataItems;

        private Thread mConnectionHealthThread;
        private bool killHealthThread = false;

/*        private void ConnectionHealth()
        {
            Console.WriteLine("Connection Health thread started; it will run every {0} seconds.", HEALTH_THREAD_SLEEP_TIME);
            while (!killHealthThread)
            {
                lock (mConnectedPlayerList)
                {
                    List<ConnectedPlayer> dcedPlayers = new List<ConnectedPlayer>();
                    foreach (ConnectedPlayer cp in mConnectedPlayerList.Values)
                    {
                        if (cp.CheckIfDCing())
                            dcedPlayers.Add(cp);
                    }

                    foreach (ConnectedPlayer cp in dcedPlayers)
                        cp.GetActor().CleanupAndSave();
                }
                Thread.Sleep(HEALTH_THREAD_SLEEP_TIME * 1000);
            }
        }*/

        public WorldServer()
        {
            mSelf = this;
        }

        public static WorldServer GetServer()
        {
            return mSelf;
        }

        public bool StartServer()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.IPv6Any, 3435);

            try
            {
                mServerSocket = new Socket(serverEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                mServerSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not Create socket, check to make sure not duplicating port", e);
            }
            try
            {
                mServerSocket.Bind(serverEndPoint);
                mServerSocket.Listen(BACKLOG);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Error occured while binding socket, check inner exception", e);
            }
            try
            {
                mServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), mServerSocket);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Error occured starting listeners, check inner exception", e);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Map Server has started @ {0}:{1}", (mServerSocket.LocalEndPoint as IPEndPoint).Address, (mServerSocket.LocalEndPoint as IPEndPoint).Port);
            Console.ForegroundColor = ConsoleColor.Gray;

            return true;
        }

        private void AcceptCallback(IAsyncResult result)
        {
            WorldClientConnection conn = null;
            Socket socket = (Socket)result.AsyncState;
            try
            {

                conn = new WorldClientConnection();
                conn.socket = socket.EndAccept(result);
                conn.buffer = new byte[BUFFER_SIZE];

                lock (mConnectionList)
                {
                    mConnectionList.Add(conn);
                }
                conn.ClientIpAddress = (conn.socket.RemoteEndPoint as IPEndPoint).Address;
                conn.ClientPort = (conn.socket.RemoteEndPoint as IPEndPoint).Port;
                Console.WriteLine("Connection {0}:{1} has connected.", (conn.socket.RemoteEndPoint as IPEndPoint).Address, (conn.socket.RemoteEndPoint as IPEndPoint).Port);
                //Queue recieving of data from the connection
                conn.socket.BeginReceive(conn.buffer, 0, conn.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), conn);
                //Queue the accept of the next incomming connection
                mServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), mServerSocket);
            }
            catch (SocketException)
            {
                if (conn != null)
                {

                    lock (mConnectionList)
                    {
                        mConnectionList.Remove(conn);
                    }
                }
                mServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), mServerSocket);
            }
            catch (Exception)
            {
                if (conn != null)
                {
                    lock (mConnectionList)
                    {
                        mConnectionList.Remove(conn);
                    }
                }
                mServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), mServerSocket);
            }
        }
/*
        public static Actor GetStaticActors(uint id)
        {
            return mStaticActors.GetActor(id);
        }

        public static Actor GetStaticActors(string name)
        {
            return mStaticActors.FindStaticActor(name);
        }

        public static Item GetItemGamedata(uint id)
        {
            if (gamedataItems.ContainsKey(id))
                return gamedataItems[id];
            else
                return null;
        }*/

        /// <summary>
        /// Receive Callback. Reads in incoming data, converting them to base packets. Base packets are sent to be parsed. If not enough data at the end to build a basepacket, move to the beginning and prepend.
        /// </summary>
        /// <param name="result"></param>
        private void ReceiveCallback(IAsyncResult result)
        {
            WorldClientConnection conn = (WorldClientConnection)result.AsyncState;
            conn.PacketProcessor = new WorldPacketProcessor();

            //Check if disconnected
            if ((conn.socket.Poll(1, SelectMode.SelectRead) && conn.socket.Available == 0))
            {
                lock (mConnectionList)
                {
                    mConnectionList.Remove(conn);
                }

                return;
            }

            try
            {
                int bytesRead = conn.socket.EndReceive(result);

                bytesRead += conn.lastPartialSize;

                if (bytesRead >= 0)
                {
                    int offset = 0;

                    //Build packets until can no longer or out of data
                    while (true)
                    {
                        BasePacket basePacket = BasePacket.CreatePacket(ref offset, conn.buffer, bytesRead);

                        //If can't build packet, break, else process another
                        if (basePacket == null)
                            break;
                        else
                        {
                            conn.PacketProcessor.ProcessPacket(conn, basePacket);
                        }

                    }

                    //Not all bytes consumed, transfer leftover to beginning
                    if (offset < bytesRead)
                        Array.Copy(conn.buffer, offset, conn.buffer, 0, bytesRead - offset);

                    conn.lastPartialSize = bytesRead - offset;

                    //Build any queued subpackets into basepackets and send
                    conn.FlushQueuedSendPackets();

                    if (offset < bytesRead)
                        //Need offset since not all bytes consumed
                        conn.socket.BeginReceive(conn.buffer, bytesRead - offset, conn.buffer.Length - (bytesRead - offset), SocketFlags.None, new AsyncCallback(ReceiveCallback), conn);
                    else
                        //All bytes consumed, full buffer available
                        conn.socket.BeginReceive(conn.buffer, 0, conn.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), conn);
                }
                else
                {

                    lock (mConnectionList)
                    {
                        mConnectionList.Remove(conn);
                    }
                }
            }
            catch (SocketException)
            {
                if (conn.socket != null)
                {

                    lock (mConnectionList)
                    {
                        mConnectionList.Remove(conn);
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
            catch (OverflowException)
            {
                return null;
            }

            return newPacket;
        }


        public static WorldManager GetWorldManager()
        {
            return mWorldManager;
        }

        public static Dictionary<uint, Item> GetGamedataItems()
        {
            return gamedataItems;
        }

        public static List<WorldClientConnection> GetClientConnections()
        {
            return mConnectionList;
        }


    }
}