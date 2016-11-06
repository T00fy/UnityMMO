using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuPrefabHandler : MonoBehaviour {

    public MenuHandler menuHandler;
    protected GameObject[] menus;
    protected GameObject[] prefabs;
    protected GameObject parentCursor;
    protected GameObject prefab;
    protected bool modalBoxOpened;
    protected bool statusBoxOpened;
    protected string modalChoice;


    void Start()
    {
        prefabs = menuHandler.GetPrefabs();
        menus = menuHandler.GetMenus();
    }

    public bool ModalBoxHasBeenOpened()
    {
        return modalBoxOpened;
    }

    public GameObject GetPrefab()
    {
        return prefab;
    }



    public bool AnsweredYes()
    {
        if (modalChoice == "Yes")
        {
            modalChoice = null;
            return true;
        }
        if(modalChoice == "No")
        {
            modalChoice = null;
            return false;
        }
        return false;
    }

    /// <summary>
    /// Instantiates a generic statusbox prefab with custom message, used mainly in character create screen
    /// </summary>
    /// <param name="prefabToInstantiate"></param>
    /// <param name="statusText"></param>
    public void InstantiatePrefab(MenuPrefabs prefabToInstantiate, string statusText)
    {
        if (prefabToInstantiate == MenuPrefabs.ModalStatusBox)
        {
            modalBoxOpened = true;
        }
        if (prefabToInstantiate == MenuPrefabs.StatusBox)
        {
            statusBoxOpened = true;
            StatusBoxHandler.readyToClose = true;
        }
        prefab = prefabs[(int)prefabToInstantiate];
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        prefab = Instantiate(prefab) as GameObject;
        menuHandler.AddMenuAsChild(prefab);
        menuHandler.SetActiveMenu(prefab);
        prefab.transform.FindChild("StatusText").GetComponentInChildren<Text>().text = statusText;
    }


    /// <summary>
    /// Instantiates a generic menu prefab directly from a gameobject
    /// </summary>
    /// <param name="characterMenu"></param>
    /// <param name="prefab"></param>
    public void InstantiatePrefab(GameObject prefabToInstantiate)
    {
        prefab = prefabToInstantiate;
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        prefab = Instantiate(prefab) as GameObject;
        menuHandler.AddMenuAsChild(prefab);
        menuHandler.SetActiveMenu(prefab);
    }

    /// <summary>
    /// Instantiates a generic menu prefab and sets it active using menuprefab enum. DON'T use this for specific menu cases like charcreate
    /// </summary>
    /// <param name="characterMenu"></param>
    /// <param name="prefab"></param>
    public void InstantiatePrefab(MenuPrefabs prefabToInstantiate)
    {
        prefab = prefabs[(int)prefabToInstantiate];
        parentCursor = menuHandler.GetCursor();
        menuHandler.ToggleCursor(false);
        prefab = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        menuHandler.AddMenuAsChild(prefab);
        menuHandler.SetActiveMenu(prefab);
    }

    /// <summary>
    /// Destroys the status box instantly
    /// </summary>
    protected void DestroyStatusBox()
    {
        GameObject parentMenu = menuHandler.GetParentMenuObject();
        menuHandler.SetActiveMenu(parentMenu);
        menuHandler.RemoveChildMenu(prefab);
        Destroy(prefab);
        if (parentCursor != null)
        {
            menuHandler.SetCursor(parentCursor);
            menuHandler.ToggleCursor(true);
        }

        statusBoxOpened = false;
        modalBoxOpened = false;
    }
}
