using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using MMOServer;

public class CharacterMenuPrefabHandler : MenuPrefabHandler {
    private GameObject priorMenu;
    private GameObject characterCreateMenu;
    private bool modalChoiceMade;
    private PacketProcessor packetProcessor;

    void Update()
    {
        if (modalBoxOpened)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                modalChoiceMade = true;
                modalChoice = menuHandler.GetCursor().GetComponent<CursorMover>().GetSelectedOption().GetComponent<Text>().text;
                DestroyBox();
            }
            if (Input.GetButtonDown("Fire2"))
            {
                modalChoiceMade = true;
                modalChoice = "No";
               DestroyBox();
            }

        }
    }

    public bool ModalSelectionMade()
    {
        return modalChoiceMade;
    }


    /// <summary>
    /// Instantiates the character create menu
    /// </summary>
    /// <param name="prefabToInstantiate"></param>
    public void InstantiatePrefabAsChangedMenu(MenuPrefabs prefabToInstantiate)
    {
        characterCreateMenu = prefabs[(int)prefabToInstantiate];
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        characterCreateMenu = Instantiate(characterCreateMenu) as GameObject;
        characterCreateMenu.transform.SetParent(GameObject.Find("MainMenu").transform);
        priorMenu = menuHandler.GetActiveMenu();
        priorMenu.SetActive(false);
        menuHandler.AddMenuAsChild(characterCreateMenu);
        menuHandler.SetMenuObject(characterCreateMenu);

    }

    public void CloseAndDiscardCharacterCreateInstance()
    {
        var oldMenu = menuHandler.GetActiveMenu();
        oldMenu.SetActive(false);
        menuHandler.SetMenuObject(menus[(int)Menus.CharacterMenu]);
        var newMenu = menuHandler.GetActiveMenu();
        newMenu.SetActive(true);
        menuHandler.SetCursor(newMenu.transform.Find("Cursor").gameObject);
        menuHandler.ToggleCursor(true);
        if (oldMenu.name == "CharacterCreation(Clone)")
        {
            menuHandler.RemoveChildMenu(characterCreateMenu);
            Destroy(oldMenu);
        }
        else
        {
            oldMenu = GameObject.Find("CharacterCreation(Clone)");
            menuHandler.RemoveChildMenu(oldMenu);
            Destroy(oldMenu);
        }

    }


    //
    //
    //shameless hack ahead
    //
    //
    public void HandleCreateDecision(GameObject[] statNumbers, GameObject nameField, ushort statsAllowed)
    {
        StartCoroutine(WaitForModalCreateDecision(statNumbers, nameField, statsAllowed));
        
    }

    public void HandleExitDecision()
    {
        StartCoroutine(WaitForModalExitDecision());

    }

    public IEnumerator WaitForModalExitDecision()
    {
        while (!modalChoiceMade)
        {
            yield return null;
        }
        Debug.Log("Choice made");
        if (AnsweredYes())
        {
            CloseAndDiscardCharacterCreateInstance();
            modalChoiceMade = false;

        }
        modalChoiceMade = false;
    }

    public IEnumerator WaitForModalCreateDecision(GameObject[] statNumbers, GameObject nameField, ushort statsAllowed)
    {
        while (!modalChoiceMade)
        {
            yield return null;
        }
        if (AnsweredYes())
        {
            modalBoxOpened = false;
            SendCharacterCreateInfo(statNumbers, nameField, statsAllowed);
            modalChoiceMade = false;

        }
        modalChoiceMade = false;
    }

    private void SendCharacterCreateInfo(GameObject[] statNumbers, GameObject nameField, ushort statsAllowed)
    {
        ushort[] stats = new ushort[statNumbers.Length];
        for (int i = 0; i < statNumbers.Length; i++)
        {
            ushort stat = ushort.Parse(statNumbers[i].GetComponent<Text>().text);
            stats[i] = stat;
        }

        //    bytesToSend = o920i
        CharacterCreatePacket cp = new CharacterCreatePacket(nameField.GetComponent<InputField>().text, stats, statsAllowed, CharacterSelect.selectedSlot);
        var bytesToSend = cp.GetData();
        SubPacket sp = new SubPacket(GamePacketOpCode.CreateCharacter, 0, 0, bytesToSend, SubPacketTypes.GamePacket);
        BasePacket characterCreationPacket = BasePacket.CreatePacket(sp, PacketProcessor.isAuthenticated, false);
        var box = gameObject.GetComponent<StatusBoxHandler>();
       // StatusBoxHandler.readyToClose = false;
       //modal status box is prefab
        box.InstantiateStatusBoxPrefabWithNoMenuLink(MenuPrefabs.StatusBox);
        StatusBoxHandler.statusText = "Waiting for response from server..";
        packetProcessor = GameObject.FindGameObjectWithTag("PacketProcessor").GetComponent<PacketProcessor>();
        packetProcessor.SendPacket(characterCreationPacket);
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
        CloseAndDiscardCharacterCreateInstance();

    }

    //
    //
    //shameless hack finished
    //
    //when writing this i didn't realise a coroutine is not allowed to be executed from an object that ever becomes deactived, due to
    //the nature of how i implemented the statusboxes and the cursor swapping between being activated and deactived,
    //i had to do it this way. Could write this better in future if I wanted to completely rewrite how prefabs become instantiated
    //but that would take way too long at this point
}
