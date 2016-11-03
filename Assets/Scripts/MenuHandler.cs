using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public GameObject characterMenu;
    public GameObject home;
    public GameObject login;
    public GameObject[] menus;

    private GameObject activeMenu;
    private GameObject previousMenu;
    private bool loginSuccessful;
    private GameObject cursor;
    private MenuTree<GameObject> root;

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

    public void AddMenuAsChild(GameObject status)
    {
        root.FindMenuTree(node => node.Data == activeMenu).AddChild(status);
    }

    public GameObject[] GetMenus()
    {
        return menus;
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

    public GameObject GetCursor() {
        return cursor;
    }

    public GameObject GetPrevious() {
        
        return previousMenu;
    }


    /// <summary>
    /// Sets the Status box okay to be destroyed after user input
    /// </summary>


    public void SetCursor(GameObject cursor)
    {
        this.cursor = cursor;
    }

    public void ToggleCursor(bool enabled)
    {
        cursor.SetActive(enabled);

    }




    void Update()
    {


    }

    public void RemoveChildMenu(GameObject menuToRemove)
    {
        root.RemoveChild(root.FindMenuTree(node => node.Data == menuToRemove));
    }

    public GameObject GetParentMenuObject()
    {
        return root.FindMenuTree(node => node.Data == activeMenu).Parent.Data;
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