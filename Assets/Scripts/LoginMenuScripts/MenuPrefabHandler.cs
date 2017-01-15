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
    public static bool statusBoxOpened;
    protected string modalChoice;
    protected bool modalChoiceMade;

    void Start()
    {
        prefabs = menuHandler.GetPrefabs();
        menus = menuHandler.GetMenus();
    }

    void Awake()
    {
        prefabs = menuHandler.GetPrefabs();
        menus = menuHandler.GetMenus();
    }

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

    public void SetModalBoxOpened()
    {
        modalBoxOpened = true;
    }

    public bool ModalBoxHasBeenOpened()
    {
        return modalBoxOpened;
    }

    public GameObject GetPrefab()
    {
        return prefab;
    }

    public bool ModalSelectionMade()
    {
        return modalChoiceMade;
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
    /// Instantiates a generic statusbox prefab with custom message that is instantly ready to close, used mainly in character create screen
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
        menuHandler.SetMenuObject(prefab);
        prefab.transform.FindChild("StatusText").GetComponentInChildren<Text>().text = statusText;
    }

    /// <summary>
    /// Instantiates a generic menu prefab directly from a gameobject, ready to close needs to be set manually
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
        menuHandler.SetMenuObject(prefab);
    }

    public void InstantiateMessageOnlyStatusBox()
    {
        prefab = prefabs[(int)MenuPrefabs.MessageOnly];
        menuHandler.ToggleCursor(false);
        prefab = Instantiate(prefab) as GameObject;
    }

    public void DestroyMessageOnlyStatusBox()
    {
        Destroy(prefab);
        menuHandler.ToggleCursor(true);
    }

    /// <summary>
    /// Instantiates a generic menu prefab and sets it active using menuprefab enum. Instantly sets it ready to close, so 
    /// used best for a quick message that shouldn't rely on other events to occur after it.
    /// DON'T use this for specific menu cases like charcreate. 
    /// </summary>
    /// <param name="characterMenu"></param>
    /// <param name="prefab"></param>
    public void InstantiatePrefab(MenuPrefabs prefabToInstantiate)
    {
        if (prefabToInstantiate == MenuPrefabs.ModalStatusBox)
        {
            modalBoxOpened = true;
        }
        if (prefabToInstantiate == MenuPrefabs.StatusBox)
        {
            statusBoxOpened = true;
        }
        prefab = prefabs[(int)prefabToInstantiate];
        menuHandler.ToggleCursor(false);
        prefab = Instantiate(prefab) as GameObject;
        menuHandler.AddMenuAsChild(prefab);
        parentCursor = menuHandler.GetCursor();
        menuHandler.SetMenuObject(prefab);
    }

    /// <summary>
    /// Destroys the status box instantly
    /// </summary>
    protected void DestroyBox()
    {
        GameObject parentMenu = menuHandler.GetParentMenuObject();
        menuHandler.SetMenuObject(parentMenu);
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
