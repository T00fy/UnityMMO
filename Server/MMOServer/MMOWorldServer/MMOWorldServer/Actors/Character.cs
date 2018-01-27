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
        public uint CharacterId { get; set; }
        public WorldClientConnection WorldClientConnection { get; set; }
        public float XPos {get; set;}
        public float YPos { get; set; }

        public float BoundsXMin { get; set; }
        public float BoundsXMax { get; set; }
        public float BoundsYMin { get; set; }
        public float BoundsYMax { get; set; }

        public string Zone { get; set; }

        public Character(uint actorID) : base(actorID)
        {
            CharacterId = actorID;
            Zone = "test"; //create a zone class later when there are more zones available
        }

        public void SavePositions(float xPos, float yPos)
        {
            XPos = xPos;
            YPos = yPos;
        }

        public void SetCharacterCameraBounds(float xMin, float xMax, float yMin, float yMax)
        {
            BoundsXMax = xMax;
            BoundsXMin = xMin;
            BoundsYMin = yMin;
            BoundsYMax = yMax;
        }

    }
}
