using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatusBoxHandler : MenuPrefabHandler {
    private bool boxOpened;
    public static Text statusTextObj;
    public static string statusText;
    public static bool readyToClose;


    void Update()
    {
        if (readyToClose && prefab != null)
        {
            if (Input.GetButtonDown("Fire1"))
            {

                DestroyStatusBox();
                if (PacketProcessor.isAuthenticated)
                {
                    menuHandler.EnterMenu(statusBoxLink);
                }
                else if (menuHandler.GetActiveMenu() == menus[(int)Menus.LoginMenu] && PacketProcessor.loggedInSuccessfully)
                {
                    menuHandler.EnterMenu(statusBoxLink);
                    PacketProcessor.loggedInSuccessfully = false;
                }
                readyToClose = false;
            }
            if (Input.GetButtonDown("Fire2"))
            {


                DestroyStatusBox();
            }
        }


        if (statusTextObj != null)
        {
            statusTextObj.text = "Status: " + statusText;
        }

    }

    public bool BoxOpened()
    {
        return boxOpened;
    }

    /// <summary>
    /// Instantiates a prefab and sets it active (set previous menu to unactive if you dont want a status box)
    /// </summary>
    /// <param name="menuLink"></param>
    /// <param name="prefab"></param>
    public void InstantiatePrefab(Menus menuLink, MenuPrefabs prefabToInstantiate)
    {
        prefab = prefabs[(int)prefabToInstantiate];
        statusBoxLink = menus[(int)menuLink];
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        prefab = Instantiate(prefab) as GameObject;
        menuHandler.AddMenuAsChild(prefab);
        statusTextObj = prefab.GetComponentInChildren<Text>();
        
        

        if (prefab.name == "ModalStatusBox")
        {
            modalBoxOpened = true;
            statusBoxOpened = true;
        }
        if (prefab.name == "StatusBox")
        {
            statusBoxOpened = true;
        }
        menuHandler.SetActiveMenu(prefab);
    }

    /// <summary>
    /// Destroys the status box instantly, without waiting for user input
    /// </summary>
    ///
    private new void DestroyStatusBox()
    {
        if (modalBoxOpened)
        {
            modalChoice = prefab.GetComponentInChildren<CursorMover>().GetSelectedOption().ToString();
        }
        GameObject parentMenu = menuHandler.GetParentMenuObject();
        menuHandler.SetActiveMenu(parentMenu);
        menuHandler.RemoveChildMenu(prefab);
        Destroy(prefab);
        if (parentCursor != null)
        {
            menuHandler.SetCursor(parentCursor);
            menuHandler.ToggleCursor(true);
        }
        statusText = "";
    }

}
