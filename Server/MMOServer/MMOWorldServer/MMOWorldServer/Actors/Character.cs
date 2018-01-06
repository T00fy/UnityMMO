using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer.Actors
{
    /// <summary>
    /// Generic class that encompasses the player and all other player characters, inherits from Actor
    /// </summary>
    class Character : Actor
    {
        public uint CharacterId {get;set;}
        public WorldClientConnection WorldClientConnection { get; set; }

        public Character(uint actorID) : base(actorID)
        {
            CharacterId = actorID;
        }


    }
}
