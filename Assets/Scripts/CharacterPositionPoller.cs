using UnityEngine;
using System.Collections;
using System;
using MMOServer;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(CharacterMovement))]
public class CharacterPositionPoller : MonoBehaviour
{
    private Character character;
    private CharacterMovement movement;
    private PositionPacket posPacket;
    private Connection connection;
    private bool posPacketReceived;
    // Use this for initialization
    void Start()
    {
        connection = GameObject.Find("WorldServerConnection").GetComponent<Connection>(); //check for performance issues
        GameEventManager.PollerPositionPacket += new GameEventManager.GameEvent(PosPacketReceived);
        character = gameObject.GetComponent<Character>();
        movement = gameObject.GetComponent<CharacterMovement>();
        InvokeRepeating("QueryForActor", 0.0f, 0.8f);
    }

    void QueryForActor()
    {
        SubPacket sp = new SubPacket(GamePacketOpCode.PositionQuery, Data.CHARACTER_ID, character.Id, new byte[0], SubPacketTypes.GamePacket);
        connection.Send(BasePacket.CreatePacket(sp, PacketProcessor.isAuthenticated, false));
    }

    void Update()
    {
        if (posPacketReceived)
        {
            HandlePosPacket();
            posPacketReceived = false;
        }
    }

    private void PosPacketReceived(GameEventArgs eventArgs)
    {
        posPacket = eventArgs.PollerPositionPacket;
        posPacketReceived = true;
    }

    private void HandlePosPacket()
    {
        if (posPacket.ActorId != character.Id)
        {
            Debug.Log("Actorid : " + posPacket.ActorId + " " + character.Id);
            throw new Exception("Uh oh, server probably not configured properly");
        }
        movement.HandleMovement(posPacket.XPos, posPacket.YPos);
    }
}
