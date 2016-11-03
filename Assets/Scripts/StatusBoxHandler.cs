using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatusBoxHandler : MonoBehaviour {
    public MenuHandler menuHandler;

    private bool boxOpened;
    public GameObject statusBoxPrefab;
    public GameObject modalStatusBoxPrefab;

    public static Text statusTextObj;
    public static string statusText;
    private GameObject statusBoxLink;
    private GameObject[] menus;
    private GameObject status;
    private bool modalBoxOpened;
    private GameObject parentCursor;
    public static bool readyToClose;
    private string modalChoice;

    void Start()
    { 
        menus = menuHandler.GetMenus();
    }

    void Update()
    {
        if (readyToClose && status != null)
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
/*
        try
        {
            if (status == null)
            {

                ToggleCursor(true);

            }
        }
        catch (NullReferenceException) { }*/

    }

    public bool BoxOpened()
    {
        return boxOpened;
    }

    //merge openstatusbox methods into one by taking an enum parameter when can be fucked to change
    public void OpenStatusBox(Menus characterMenu)
    {
        statusBoxLink = menus[(int)characterMenu];
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        status = Instantiate(statusBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        menuHandler.AddMenuAsChild(status);
        statusTextObj = status.GetComponentInChildren<Text>();
        boxOpened = true;
        menuHandler.SetActiveMenu(status);
    }

    public void OpenModalStatusBox(Menus characterMenu)
    {
        statusBoxLink = menus[(int)characterMenu];
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        status = Instantiate(modalStatusBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        menuHandler.AddMenuAsChild(status);
        statusTextObj = status.GetComponentInChildren<Text>();
        modalBoxOpened = true;
        menuHandler.SetActiveMenu(status);
    }

    public bool GetModalChoice()
    {
        if (modalChoice == "Yes")
        {
            modalChoice = null;
            return true;
        }
        else
        {
            modalChoice = null;
            return false;
        }
    }

    /// <summary>
    /// Destroys the status box instantly, without waiting for user input
    /// </summary>
    private void DestroyStatusBox()
    {
        if (modalBoxOpened)
        {
            modalChoice = status.GetComponentInChildren<CursorMover>().GetSelectedOption().ToString();
        }
        GameObject parentMenu = menuHandler.GetParentMenuObject();
        menuHandler.SetActiveMenu(parentMenu);
        menuHandler.RemoveChildMenu(status);
        Destroy(status);
        if (parentCursor != null)
        {
            menuHandler.SetCursor(parentCursor);
            menuHandler.ToggleCursor(true);
        }
        statusText = "";
    }

}
