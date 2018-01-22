using System;
using System.Collections;
using System.Collections.Generic;
using MMOServer;
using UnityEngine;

public class WorldPacketProcessor : Processor
{
    private bool isAuthenticated;
    private Connection connection;
    private CharacterDestroyer destroyer;

    void Awake()
    {
        connection = GameObject.Find("WorldServerConnection").GetComponent<Connection>();
        connection.SetPacketProcessor(this);
    }

    void Start()
    {
        destroyer = GameObject.Find("CharacterDestroyer").GetComponent<CharacterDestroyer>();
    }

    public override void ProcessPacket(BasePacket receivedPacket)
    {
        if (!isAuthenticated && receivedPacket.isAuthenticated())
        {
            isAuthenticated = true;
        }
        List<SubPacket> subPackets = receivedPacket.GetSubpackets();
        foreach (SubPacket subPacket in subPackets)
        {
            DoAuthenticationChecks(receivedPacket, subPacket);

            if (!receivedPacket.isAuthenticated())
            {
                Debug.Log("Not authenticated.. Do something here");
                throw new NotImplementedException();
            }
            else
            {
                switch (subPacket.gameMessage.opcode)
                {
                    case ((ushort)GamePacketOpCode.NearbyActorsQuery):
                        PositionPacket pos = new PositionPacket(subPacket.data);
                        ActorWrapper wrapper = new ActorWrapper(pos.XPos, pos.YPos, pos.Playable, pos.ActorId);
                        GameEventManager.TriggerActorNeedsDrawing(new GameEventArgs { Actor = wrapper });

                        break;

                    case ((ushort)GamePacketOpCode.PositionQuery):
                        PositionPacket otherCharacterPos = new PositionPacket(subPacket.data);
                        GameEventManager.TriggerPollerResponse(new GameEventArgs { PollerPositionPacket = otherCharacterPos });

                        break;

                    case ((ushort)GamePacketOpCode.OtherPlayerDisconnected):
                        DisconnectPacket dc = new DisconnectPacket(subPacket.data);
                        Character playerToDisconnect;
                        if (Data.drawnCharacters.TryGetValue(dc.CharacterId, out playerToDisconnect))
                        {
                            Data.drawnCharacters.Remove(dc.CharacterId);
                            destroyer.AddCharacter(playerToDisconnect);
                        }
                        break;

                }
            }
        }
    }

    public override void DoAuthenticationChecks(BasePacket receivedPacket, SubPacket subPacket)
    {
        if (isAuthenticated && !receivedPacket.isAuthenticated() && subPacket.gameMessage.opcode != (ushort)GamePacketOpCode.RegisterSuccess)
        {
            isAuthenticated = false;
        }
    }


}
