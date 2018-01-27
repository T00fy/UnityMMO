using MMOServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnterWorld : MonoBehaviour
{
    public Connection worldServerConnection;
    public Connection loginServerConnection;
    public MenuPrefabHandler genericBoxHandler;
    public StatusBoxHandler statusBoxHandler;
    private bool? handShakeSuccessful;
    private bool clientActivatedEnterWorld;
    public float handshakeTimeoutWait = 15.0f;
    private CharacterSelect characterSelect;
    private Character characterEntering;

    // Use this for initialization
    void Start()
    {
        GameEventManager.HandshakeResponse += new GameEventManager.GameEvent(ServerResponse);
        GameEventManager.ClientWantsToEnter += new GameEventManager.GameEvent(InputFromCharacterSelect);
        GameEventManager.StatusBoxClosed += new GameEventManager.GameEvent(StatusBoxResponse);
        DontDestroyOnLoad(worldServerConnection);
    }

    private void ServerResponse(GameEventArgs eventArgs)
    {
        handShakeSuccessful = eventArgs.ServerResponse;
    }

    private void InputFromCharacterSelect(GameEventArgs eventArgs)
    {
        clientActivatedEnterWorld = eventArgs.ClientSelectedEnterWorld;
        var selectGameObject = Utils.FindSiblingGameObjectByName(gameObject, "CharacterSlots");
        characterSelect = selectGameObject.GetComponent<CharacterSelect>();
        characterSelect.enabled = false;

    }

    private void StatusBoxResponse(GameEventArgs eventArgs)
    {
        try
        {
            characterSelect.enabled = true;
        }
        catch (NullReferenceException) { } //this is a hack until I change all instances of 'statusboxclosed' boolean to using an event
        
    }

    // Update is called once per frame
    void Update()
    {
        if (clientActivatedEnterWorld) //if is entering
        {
            worldServerConnection.EstablishConnection(Data.WORLD_ADDRESS, Data.WORLD_PORT);
            characterEntering = Utils.GetCharacter(CharacterSelect.selectedSlot);
            Data.CHARACTER_ID = (uint)characterEntering.Id;
            var characterIdBytes = BitConverter.GetBytes(characterEntering.Id);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(characterIdBytes);
            }
            SubPacket packetToSend = new SubPacket(GamePacketOpCode.Handshake, Data.CHARACTER_ID, 0, characterIdBytes, SubPacketTypes.GamePacket);
            BasePacket test = BasePacket.CreatePacket(packetToSend, PacketProcessor.isAuthenticated, false);
            test.header.connectionType = (ushort)BasePacketConnectionTypes.Connect;
            worldServerConnection.Send(test);
            genericBoxHandler.InstantiateMessageOnlyStatusBox();
            var boxText = genericBoxHandler.GetPrefab().GetComponentInChildren<Text>();
            boxText.text = "Handshaking with server..";

            StartCoroutine(WaitForServerResponse());
            clientActivatedEnterWorld = false;
        }
    }

    private IEnumerator WaitForServerResponse()
    {
        while (!handShakeSuccessful.HasValue)
        {
            handshakeTimeoutWait -= Time.deltaTime;
            if (handshakeTimeoutWait < 0)
            {
                break;
            }
            yield return null;
        }
        if (handShakeSuccessful.HasValue)
        {
            if ((bool)handShakeSuccessful)
            {
                Data.CHARACTER_ON_LOGIN = characterEntering;
                loginServerConnection.SendDisconnectPacket();
                SceneManager.LoadScene("test", LoadSceneMode.Single);
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
