using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this script to an actor object which you expect to be able to move.
/// </summary>
public class MovementRelayer : MonoBehaviour {

    private bool actorMoving;
    private PlayerMovement mover;
    private Connection connection;

	void Start () {
        connection = GameObject.Find("WorldPacketProcessor").GetComponent<Connection>();
        mover = gameObject.GetComponent<PlayerMovement>();
        InvokeRepeating("CheckMovement", 0.0f, 0.03f);

    }

    private void CheckMovement()
    {
        if (mover.IsMoving)
        {
            SendPositionPacket();
        }
    }

    private void SendPositionPacket()
    {
        //PositionPacket posPacket = new PositionPacket();
    }	
}
