using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using MMOServer;

public class CharacterDeleteHandler : MenuPrefabHandler
{
    private uint characterID;
    private Character character;
    // Use this for initialization
    void Start()
    {

    }

    public void HandleDeleteDecision(Character character)
    {
        this.character = character;
        characterID = character.Id;
        StartCoroutine(WaitForDeleteConfirmation());
    }

    private IEnumerator WaitForDeleteConfirmation()
    {
        while (!modalChoiceMade)
        {
            yield return null;
        }
        if (AnsweredYes())
        {
            modalBoxOpened = false;
            SendDeleteCharacterRequest();
        }
        modalChoiceMade = false;
    }

    private void SendDeleteCharacterRequest()
    {
        CharacterDeletePacket cd = new CharacterDeletePacket(characterID);
        SubPacket sp = cd.GetQueryPacket();
        BasePacket packetToSend = BasePacket.CreatePacket(sp, PacketProcessor.isAuthenticated, false);

        var box = gameObject.GetComponent<StatusBoxHandler>();
        box.InstantiateStatusBoxPrefabWithNoMenuLink(MenuPrefabs.StatusBox);
        StatusBoxHandler.statusText = "Waiting for response from server..";
        PacketProcessor packetProcessor = GameObject.FindGameObjectWithTag("PacketProcessor").GetComponent<PacketProcessor>();
        packetProcessor.SendPacket(packetToSend);
        StartCoroutine(WaitForServerResponse(box));
    }

    private IEnumerator WaitForServerResponse(StatusBoxHandler box)
    {
        while (!StatusBoxHandler.readyToClose)
        {
            yield return null;
        }
        while (box.GetPrefab() != null)
        {
            yield return null;
        }
        //refresh character select window here?
        var characterInfo = Utils.FindSiblingGameObjectByTag(character.transform.parent.gameObject, "CharacterInfo");
        characterInfo.GetComponent<Text>().text = "";
        Destroy(character.gameObject);
        CharacterSelect.deleting = false;
    }
}
