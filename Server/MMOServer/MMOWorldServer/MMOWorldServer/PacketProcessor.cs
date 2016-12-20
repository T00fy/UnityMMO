using MMOServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer
{
    class PacketProcessor
    {
        WorldServer mServer;
 //       CommandProcessor cp;
        Dictionary<uint, ConnectedPlayer> mPlayers;
        List<ClientConnection> mConnections;

        public PacketProcessor(WorldServer server, Dictionary<uint, ConnectedPlayer> playerList, List<ClientConnection> connectionList)
        {
            mPlayers = playerList;
            mConnections = connectionList;
            mServer = server;
 //           cp = new CommandProcessor(playerList);
        }

        public void ProcessPacket(ClientConnection client, BasePacket packet)
        { }

        }
}
