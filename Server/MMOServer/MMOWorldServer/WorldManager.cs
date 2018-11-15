using MMOWorldServer.Actors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MMOWorldServer
{
    /// <summary>
    /// This class will be used for all world loading:
    /// loading the world into a list from DB
    /// Controlling seamless boundaries in zones
    /// Placing NPCS into the world
    /// Loading spawn locations including logins
    /// Checking if positions are within bounds 
    /// </summary>
    class WorldManager
    {
        public void InitCharacterPositionsSaver()
        {
            Timer timer = new Timer(SaveCharacterPositions, WorldServer.mConnectedPlayerList, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            Thread.Sleep(30000); // Wait 30 seconds.

        }

        private void SaveCharacterPositions(object state)
        {
            ConcurrentDictionary<uint, Character> players = (ConcurrentDictionary<uint, Character>)state;
            foreach (var entry in players)
            {
                var character = entry.Value;
                Console.WriteLine("Saving character position for: " + character.CharacterId);
                WorldDatabase.UpdateCharacterPosition(character.CharacterId, character.XPos, character.YPos, character.Zone);
            }
        }
    }
}
