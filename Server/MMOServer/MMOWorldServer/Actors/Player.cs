using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer.Actors
{
    /// <summary>
    /// Class that has all the methods relevant to player actions, inherits from Character
    /// </summary>
    class Player : Character
    {

        uint actorId;
        WorldClientConnection playerSession;

        public Player(WorldClientConnection cp, uint actorId) : base(actorId)
        {
            playerSession = cp;
            this.actorId = actorId;
        }

        public void CleanupAndSave()
        {
            throw new NotImplementedException();
            //zone.RemoveActorFromZone(this);
            //Server.GetServer().RemovePlayer(this);

            //Save Player
            //Database.SavePlayerPlayTime(this);
            //Database.SavePlayerPosition(this);
        }
    }
}
