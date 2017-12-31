using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatusBoxHandler : MenuPrefabHandler {
    public static Text statusTextObj;
    public static string statusText;
    public static bool readyToClose;
    private GameObject statusBoxLink;


    void Update()
    {
        if (readyToClose && prefab != null)
        {
            if (Input.GetButtonDown("Fire1"))
            {

                DestroyStatusBox();
                if (PacketProcessor.isAuthenticated)
                {
                    if (statusBoxLink != null)
                    {
                        menuHandler.EnterMenu(statusBoxLink);
                    }
                    
                }
                else if (menuHandler.GetActiveMenu() == menus[(int)Menus.LoginMenu] && PacketProcessor.loggedInSuccessfully)
                {
                    menuHandler.EnterMenu(statusBoxLink);
                    PacketProcessor.loggedInSuccessfully = false;
                }
                GameEventManager.TriggerStatusBoxClosed(new GameEventArgs { statusBoxClosed = true });
                readyToClose = false;
                statusBoxLink = null;
            }
            if (Input.GetButtonDown("Fire2"))
            {


                DestroyStatusBox();
                readyToClose = false;
                statusBoxLink = null;
            }
            
        }


        if (statusTextObj != null)
        {
            statusTextObj.text = "Status: " + statusText;
        }

    }

    /// <summary>
    /// Instantiates a prefab and sets it active, goes to the specified menulink when OK is pushed
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

        if (prefab.name == "StatusBox(Clone)")
        {
            statusBoxOpened = true;
        }
        menuHandler.SetMenuObject(prefab);
    }

    public void InstantiateStatusBoxPrefabWithNoMenuLink(MenuPrefabs prefabToInstantiate)
    {
        prefab = prefabs[(int)prefabToInstantiate];
        parentCursor = menuHandler.GetActiveMenu().transform.Find("Cursor").gameObject;
        menuHandler.SetCursor(parentCursor);
        menuHandler.ToggleCursor(false);
        prefab = Instantiate(prefab) as GameObject;
        menuHandler.AddMenuAsChild(prefab);
        statusTextObj = prefab.GetComponentInChildren<Text>();
        if (prefab.name == "StatusBox(Clone)")
        {
            statusBoxOpened = true;
        }
        menuHandler.SetMenuObject(prefab);
    }

    /// <summary>
    /// Destroys the status box instantly, without waiting for user input
    /// </summary>
    ///
    public void DestroyStatusBox()
    {
  /*      if (modalBoxOpened)
        {
            modalChoice = prefab.GetComponentInChildren<CursorMover>().GetSelectedOption().ToString();
        }*/
        GameObject parentMenu = menuHandler.GetParentMenuObject();
        menuHandler.SetMenuObject(parentMenu);
        menuHandler.RemoveChildMenu(prefab);
        Destroy(prefab);
        if (parentCursor != null)
        {
            menuHandler.SetCursor(parentCursor);
            menuHandler.ToggleCursor(true);
        }
        statusText = "";
        statusBoxOpened = false;
    }

}
