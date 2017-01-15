using MMOServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterWorld : MonoBehaviour
{
    //    public CursorMover cm;
    public Connection worldServerConnection;
    public MenuPrefabHandler genericBoxHandler;
    public StatusBoxHandler statusBoxHandler;
    private bool? handShakeSuccessful;
    private bool clientActivatedEnterWorld;
    private const float TIMEOUT_DURATION = 30.0f;
    private bool enterHandshakeStatusBoxClosed;
    private CharacterSelect characterSelect;

    // Use this for initialization
    void Start()
    {
        GameEventManager.HandshakeResponse += new GameEventManager.GameEvent(ServerResponse);
        GameEventManager.ClientWantsToEnter += new GameEventManager.GameEvent(InputFromCharacterSelect);
        GameEventManager.StatusBoxClosed += new GameEventManager.GameEvent(StatusBoxResponse);
    }

    private void ServerResponse(GameEventArgs eventArgs)
    {
        handShakeSuccessful = eventArgs.serverResponse;
    }

    private void InputFromCharacterSelect(GameEventArgs eventArgs)
    {
        clientActivatedEnterWorld = eventArgs.clientSelectedEnterWorld;
        var selectGameObject = Utils.FindSiblingGameObjectByName(gameObject, "CharacterSlots");
        characterSelect = selectGameObject.GetComponent<CharacterSelect>();
        characterSelect.enabled = false;

    }

    private void StatusBoxResponse(GameEventArgs eventArgs)
    {
        enterHandshakeStatusBoxClosed = eventArgs.statusBoxClosed;
        characterSelect.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (clientActivatedEnterWorld) //if is entering
        {
            worldServerConnection.EstablishConnection("127.0.0.1", 3435);
            //   EnterWorldPacket packet = new EnterWorldPacket()
            int characterId = Utils.GetCharacter(CharacterSelect.selectedSlot).CharId;
            var characterIdBytes = BitConverter.GetBytes(characterId);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(characterIdBytes);
            }
            SubPacket packetToSend = new SubPacket(GamePacketOpCode.Handshake, 0, 0, characterIdBytes, SubPacketTypes.GamePacket);
            BasePacket test = BasePacket.CreatePacket(packetToSend, PacketProcessor.isAuthenticated, false);
            test.header.connectionType = (ushort)BasePacketConnectionTypes.Generic;
            worldServerConnection.Send(test);
            genericBoxHandler.InstantiateMessageOnlyStatusBox();
            var boxText = genericBoxHandler.GetPrefab().GetComponentInChildren<Text>();
            boxText.text = "Handshaking with server..";

            StartCoroutine(WaitForServerResponse());
            clientActivatedEnterWorld = false;
        }
    }

    public void StartEnteringWorld()
    {

    }

    private IEnumerator WaitForServerResponse()
    {
        while (true)
        {
            yield return new WaitForSeconds(TIMEOUT_DURATION);
            break;
        }
        if (handShakeSuccessful.HasValue)
        {
            if ((bool)handShakeSuccessful)
            {
                Debug.Log("Load World");
                //load world instantly
            }
            else
            {
                Debug.Log("Received response but couldn't authenticate you with login server");
                //make status box with message saying something messed up
            }
        }
        else
        {
            genericBoxHandler.DestroyMessageOnlyStatusBox();
            statusBoxHandler.InstantiatePrefab(MenuPrefabs.StatusBox, "Connection to server timed out");
        }


    }
}
