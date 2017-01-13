using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMOServer;
using MMOWorldServer.Actors;

namespace MMOWorldServer
{
    /// <summary>
    /// Will have all related player connection methods
    /// Class will encapsulate two ClientConnection types for play: zoneConnection and chatConnection
    /// Have methods to set these two clientconnections
    /// Will also include player connection methods like updating player position
    /// </summary>
    class ConnectedPlayer
    {
        public ClientConnection zoneConnection;
        public ClientConnection chatConnection;
        public uint actorId = 0;

        Player playerActor;
        public List<Actor> actorInstanceList = new List<Actor>();

        private uint lastPingPacket = Utils.UnixTimeStampUTC();
        private string clientAddress;

        public string errorMessage = "";

        public string ClientAddress
        {
            get
            {
                return clientAddress;
            }

            set
            {
                clientAddress = value;
            }
        }

        public ConnectedPlayer(int actorId)
        {
            this.actorId = (uint)actorId;
            playerActor = new Player(this, (uint)actorId);
            actorInstanceList.Add(playerActor);
        }

        public ConnectedPlayer(uint actorId)
        {
            this.actorId = actorId;
            playerActor = new Player(this, actorId);
            actorInstanceList.Add(playerActor);
        }

        public void SetConnection(int type, ClientConnection conn)
        {
            conn.connType = type;
            switch (type)
            {
                case BasePacket.TYPE_ZONE:
                    zoneConnection = conn;
                    break;
                case BasePacket.TYPE_CHAT:
                    chatConnection = conn;
                    break;
            }
        }

        public bool IsClientConnectionsReady()
        {
            return (zoneConnection != null && chatConnection != null);
        }

        public void Disconnect()
        {
            zoneConnection.Disconnect();
            chatConnection.Disconnect();
        }

        public bool IsDisconnected()
        {
            return (!zoneConnection.IsConnected() && !chatConnection.IsConnected());
        }

        public void QueuePacket(BasePacket basePacket)
        {
            zoneConnection.QueuePacket(basePacket);
        }

        public void QueuePacket(SubPacket subPacket, bool isAuthed, bool isEncrypted)
        {
            zoneConnection.QueuePacket(subPacket, isAuthed, isEncrypted);
        }

        public Player GetActor()
        {
            return playerActor;
        }

        public void Ping()
        {
            lastPingPacket = Utils.UnixTimeStampUTC();
        }

        public bool CheckIfDCing()
        {
            throw new NotImplementedException();
        /*    uint currentTime = Utils.UnixTimeStampUTC();
            if (currentTime - lastPingPacket >= 5000) //Show D/C flag
                playerActor.SetDCFlag(true);
            else if (currentTime - lastPingPacket >= 30000) //DCed
                return true;
            else
                playerActor.SetDCFlag(false);
            return false;*/
        }

        public void UpdatePlayerActorPosition(float x, float y, ushort moveState)
        {
            throw new NotImplementedException();
         /*   playerActor.oldPositionX = playerActor.positionX;
            playerActor.oldPositionY = playerActor.positionY;
            playerActor.oldPositionZ = playerActor.positionZ;
            playerActor.oldRotation = playerActor.rotation;

            playerActor.positionX = x;
            playerActor.positionY = y;
            playerActor.moveState = moveState;

            GetActor().zone.UpdateActorPosition(GetActor());*/

        }

    }
}
