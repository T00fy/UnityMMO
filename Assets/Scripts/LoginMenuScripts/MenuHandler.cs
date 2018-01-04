using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public GameObject characterMenu;
    public GameObject home;
    public GameObject login;
    public GameObject[] menus;
    public GameObject[] prefabs;

    private GameObject activeMenu;
    private GameObject previousMenu;
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
            var loginMenu = root.AddChild(menus[(int)Menus.LoginMenu]);
            {
                var characterMenu = loginMenu.AddChild(menus[(int)Menus.LoginMenu]);
                {
                    var characterSelection = characterMenu.AddChild(menus[(int)Menus.CharacterMenu]);//character creation
                }
            } //login

            var registerMenu = root.AddChild(menus[(int)Menus.RegisterMenu]); //register
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
    public GameObject[] GetPrefabs()
    {
        return prefabs;
    }

    public void SetMenuObject(GameObject current)
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


    public void SetCursor(GameObject cursor)
    {
        this.cursor = cursor;
    }

    public void ToggleCursor(bool enabled)
    {
        cursor.SetActive(enabled);

    }

    public void RemoveChildMenu(GameObject menuToRemove)
    {
        root.RemoveChild(root.FindMenuTree(node => node.Data == menuToRemove));
    }

    public GameObject GetParentMenuObject()
    {
        return root.FindMenuTree(node => node.Data == activeMenu).Parent.Data;
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
        if (activeMenu.name != "CharacterCreation(Clone)")
        {
            activeMenu.SetActive(false);
            try
            {
                var parent = root.FindMenuTree(node => node.Data == activeMenu).Parent.Data;
                activeMenu = parent;
            }
            catch (NullReferenceException)
            {
                Debug.Log("Missing menu");
            }

            activeMenu.SetActive(true);
        }
    }
}