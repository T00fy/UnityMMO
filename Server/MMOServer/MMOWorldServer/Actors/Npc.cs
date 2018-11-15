using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer.Actors
{
    /// <summary>
    /// Class that has all methods related to NPC actions, inherits from Actor
    /// </summary>
    class Npc : Actor
    {
        public Npc(int actorNumber, /*ActorClass actorClass,*/ string uniqueId, uint zoneId, float posX, float posY, ushort actorState, uint animationId, string customDisplayName)
    : base((4 << 28 | zoneId << 19 | (uint)actorNumber))
        {


        }
    }
}
