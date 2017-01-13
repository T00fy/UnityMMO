using MMOServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterWorld : MonoBehaviour {
    public CursorMover cm;
    public Connection worldServerConnection;
    public MenuPrefabHandler handler;
    private bool? handShakeSuccessful;
	// Use this for initialization
	void Start () {
        GameEventManager.HandshakeResponse += new GameEventManager.GameEvent(ServerResponse);
	}

    private void ServerResponse(GameEventArgs eventArgs)
    {
        handShakeSuccessful = eventArgs.serverResponse;
    }

    // Update is called once per frame
    void Update () {
        if (cm.GetSelectedOption() == cm.menuObjects[4] && Input.GetButtonDown("Fire1")) //if is entering
        {

            worldServerConnection.EstablishConnection("127.0.0.1", 3435);
         //   EnterWorldPacket packet = new EnterWorldPacket()
            int characterId = Utils.GetCharacter(CharacterSelect.selectedSlot).CharId;
            Debug.Log(characterId);
            var characterIdBytes = BitConverter.GetBytes(characterId);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(characterIdBytes);
            }
            SubPacket packetToSend = new SubPacket(GamePacketOpCode.Handshake, 0, 0, characterIdBytes, SubPacketTypes.GamePacket);
            BasePacket test = BasePacket.CreatePacket(packetToSend, PacketProcessor.isAuthenticated, false);
            test.header.connectionType = (ushort)BasePacketConnectionTypes.Generic;
            worldServerConnection.Send(test);
            handler.InstantiateMessageOnlyStatusBox();
            var boxText = handler.GetPrefab().GetComponentInChildren<Text>();
            boxText.text = "Handshaking with server..";
            StartCoroutine(WaitForServerResponse());
            
            
        }
	}

    private IEnumerator WaitForServerResponse()
    {
        while (!handShakeSuccessful.HasValue)
        {
            yield return null;
            //give time out of 20 seconds
            //if timed out, destroy old status box and create new normal one with cursor.
        }
        if ((bool)handShakeSuccessful)
        {
            Debug.Log("Load World");
            //load world instantly
        }
        else
        {
            Debug.Log("Don't load world");
            //make status box with message saying something messed up

        }
    }
}
