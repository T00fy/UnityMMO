using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public GameObject statusBoxPrefab;
    public GameObject modalStatusBoxPrefab;
    public GameObject characterMenu;
    public GameObject home;
    public GameObject login;
    public GameObject[] menus;

    private GameObject activeMenu;
    private GameObject previousMenu;
    private bool loginSuccessful;
    private GameObject status;
    private Text statusTextObj;
    private string statusText;
    private bool boxOpened;
    private GameObject cursor;
    private bool finishedConnection;
    private MenuTree<GameObject> root;
    private GameObject statusBoxLink;
    private bool modalBoxOpened;
    private string modalChoice;

    void Start()
    {
        InitializeMenuHeirarchy();
    }

    private void InitializeMenuHeirarchy()
    {
        root = new MenuTree<GameObject>(home);
        {
            var loginMenu = root.AddChild(menus[1]);
            {
                var characterMenu = loginMenu.AddChild(menus[2]);
                {
                    var characterSelection = characterMenu.AddChild(menus[3]);//character creation
                }
            } //login

            var registerMenu = root.AddChild(menus[4]); //register
        }
        activeMenu = root.Data;
    }

    public void SetActiveMenu(GameObject current)
    {
        activeMenu = current;
    }

    public GameObject GetActiveMenu()
    {
        return activeMenu;
    }

    public void SetPrevious(GameObject activeMenu) {
        previousMenu = activeMenu;
    }

    public GameObject ThisCursor() {
        return cursor;
    }

    public GameObject GetPrevious() {
        
        return previousMenu;
    }

    public bool BoxOpened() {
        return boxOpened;
    }

    public void OpenStatusBox(Menus characterMenu)
    {
        statusBoxLink = menus[(int)characterMenu];
        ToggleCursor(false);
        status = Instantiate(statusBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        root.FindMenuTree(node => node.Data == activeMenu).AddChild(status);
        statusTextObj = status.GetComponentInChildren<Text>();
        //     boxOpened = true;
        activeMenu = status;
    }

    public void OpenModalStatusBox(Menus characterMenu)
    {
        statusBoxLink = menus[(int)characterMenu];
        ToggleCursor(false);
        status = Instantiate(modalStatusBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        root.FindMenuTree(node => node.Data == activeMenu).AddChild(status);
        statusTextObj = status.GetComponentInChildren<Text>();
        modalBoxOpened = true;
        activeMenu = status;
    }


    /// <summary>
    /// Sets the Status box okay to be destroyed after user input
    /// </summary>
    public void SetDestroyStatusBox() {
        finishedConnection = true;

    }

    public void SetCursor(GameObject cursor)
    {
        this.cursor = cursor;
    }

    public void ToggleCursor(bool enabled)
    {
        cursor.SetActive(enabled);

    }

    public void SetStatusText(string statusText)
    {
        this.statusText = statusText;
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
        activeMenu = root.FindMenuTree(node => node.Data == activeMenu).Parent.Data;
        var blah = activeMenu.transform.FindChild("Cursor");
        SetCursor(blah.gameObject);
        root.RemoveChild(root.FindMenuTree(node => node.Data == activeMenu));
        Destroy(status);
        SetStatusText("");
    }


    void Update()
    {
        if (finishedConnection && status != null)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                
                DestroyStatusBox();
                if (loginSuccessful)
                {
                    EnterMenu(statusBoxLink);
                    loginSuccessful = false;
                }
                if (activeMenu != menus[(int)Menus.LoginMenu] && activeMenu != menus[(int)Menus.RegisterMenu])
                {
                    EnterMenu(statusBoxLink);
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

        try
        {
            if (status == null)
            {

                ToggleCursor(true);

            }
        }
        catch (NullReferenceException) { }
        



    }

    public void LoggedInSuccessfully()
    {
        loginSuccessful = true;
    }

    public void EnterMenu(GameObject enterMenu)
    {
        
        activeMenu.SetActive(false);
        activeMenu = root.FindMenuTree(node => node.Data == enterMenu).Data;
        if (activeMenu != null)
        {
            activeMenu.SetActive(true);
        }
        else
        {
            Debug.Log("Something fucked up, menu is null");
        }
        
    }

    public void GoUpMenu()
    {
        if (status != null)
            DestroyStatusBox();
        activeMenu.SetActive(false);
        try
        {
            var parent = root.FindMenuTree(node => node.Data == activeMenu).Parent.Data;
            activeMenu = parent;
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Missing menu");
        }
        
        
        activeMenu.SetActive(true);
    }
}