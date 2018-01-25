using MMOServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Attach this script to an actor object which you expect to be able to move.
/// </summary>
public class MovementRelayer : MonoBehaviour {

    private bool actorMoving;
    private PlayerMovement mover;
    private Connection connection;
    public float networkTick = 0.01f;

    void Start()
    {
        connection = GameObject.Find("WorldServerConnection").GetComponent<Connection>();
        mover = gameObject.GetComponent<PlayerMovement>();
   //     InvokeRepeating("RelayMovement", 0.0f, Time.deltaTime * 5);

    }

    void Update()
    {
        RelayMovement();
    }

    void RelayMovement()
    {
            PositionPacket posPacket = new PositionPacket(gameObject.transform.position.x, gameObject.transform.position.y, true, Data.CHARACTER_ID);
            SubPacket sp = new SubPacket(GamePacketOpCode.PositionPacket, Data.CHARACTER_ID, 0, posPacket.GetBytes(), SubPacketTypes.GamePacket);
            connection.Send(BasePacket.CreatePacket(sp,true,false));
        }
    }
