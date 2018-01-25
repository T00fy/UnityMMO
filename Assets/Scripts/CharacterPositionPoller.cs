using UnityEngine;
using System.Collections;
using System;
using MMOServer;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(CharacterMovement))]
public class CharacterPositionPoller : MonoBehaviour
{
    public float framesPerQuery = 10;
    private Character character;
    private CharacterMovement movement;
    private PositionPacket posPacket;
    private Connection connection;
    // Use this for initialization
    void Start()
    {
        connection = GameObject.Find("WorldServerConnection").GetComponent<Connection>(); //check for performance issues
        GameEventManager.PollerPositionPacket += new GameEventManager.GameEvent(PosPacketReceived);
        character = gameObject.GetComponent<Character>();
        movement = gameObject.GetComponent<CharacterMovement>();
       // InvokeRepeating("UpdateActorPosition", 0.0f, Time.deltaTime * framesPerQuery);
    }

    void UpdateActorPosition()
    {
        SubPacket sp = new SubPacket(GamePacketOpCode.PositionQuery, Data.CHARACTER_ID, character.Id, new byte[0], SubPacketTypes.GamePacket);
        connection.Send(BasePacket.CreatePacket(sp, PacketProcessor.isAuthenticated, false));
    }

    void Update()
    {
        if (posPacket != null)
        {
            if (posPacket.ActorId != character.Id)
            {
                Debug.Log("Actorid : " + posPacket.ActorId + " " + character.Id);
                throw new Exception("Uh oh, server probably not configured properly");
            }
            movement.HandleMovement(posPacket.XPos, posPacket.YPos);
        }
        UpdateActorPosition();
    }

    private void PosPacketReceived(GameEventArgs eventArgs)
    {
        if (eventArgs.PollerPositionPacket.ActorId == character.Id)
        {
            posPacket = eventArgs.PollerPositionPacket;
        }
    }
}
