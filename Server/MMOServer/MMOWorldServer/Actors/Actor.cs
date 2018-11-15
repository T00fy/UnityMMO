using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer.Actors
{
    /// <summary>
    /// Generic class for NPCS and players to inherit from
    /// </summary>
    class Actor
    {
        uint actorId;

        public Actor(uint actorId)
        {
            this.actorId = actorId;
        }


    }
}
