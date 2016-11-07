using UnityEngine;
using System;
using UnityEngine.UI;
using MMOServer;

public class CursorInput : MonoBehaviour {
    public static MenuHandler menuHandler;

    private GameObject cursor;
    private GameObject activeMenu;
    private bool enteringText;
    private InputField inputField;
    private CursorMover cm;
    private GameObject selectedOption;


    void Awake () {
        menuHandler = GameObject.Find("MenuHandler").GetComponent<MenuHandler>();
        cursor = gameObject;
        string parent = cursor.transform.parent.name;
        cm = gameObject.GetComponent<CursorMover>();
        selectedOption = cm.GetSelectedOption();
        menuHandler.SetCursor(cursor);
    }

    void OnEnable()
    {
        menuHandler.SetCursor(cursor);
    }

	
	void Update () {
        Vector2 direction = new Vector2(0,0);
        bool inputPressed = false;
        if (Input.GetButtonDown("Vertical") || Input.GetButtonDown("Horizontal"))
        {
            direction = direction + getDirection();
            enteringText = false;
            inputPressed = true;
      //      Debug.Log("DirectoinV: " + direction);

        }

        if (inputPressed) {
            cm.SetDirection(direction);
        }

        selectedOption = cm.GetSelectedOption();
        string clientState = "";
        MenuLink ml = selectedOption.GetComponent<MenuLink>();

        if (ml != null) {
            clientState = ml.GetState().ToString();
        }

        if (clientState == "inputfield")
        {
            inputField = ml.GetComponent<InputField>();
            
            enteringText = true;
            inputField.enabled = true;
            inputField.ActivateInputField();

        }
        else {
            enteringText = false;
            if (inputField != null) {
                inputField.enabled = false;
            }
            
        }

        if (!enteringText) {
            if (Input.GetButtonDown("Fire1"))
            {
                
                switch (clientState) {
                    
                    case "menu":
                        menuHandler.EnterMenu(ml.GetMenuItem());
                        break;

                    case "register":
                        //   SubmitAccount("UsernameRegister", "PasswordRegister", ml, true);
                        var submitobj = GameObject.Find("Submit");
                        var accountHandler = submitobj.GetComponent<AccountHandler>();
                        accountHandler.SubmitAccount("UsernameRegister", "PasswordRegister", ml, true);
                        break;

                    case "login":
                        //    SubmitAccount("UsernameLogin", "PasswordLogin", ml, false);
                        submitobj = GameObject.Find("Submit");
                        accountHandler = submitobj.GetComponent<AccountHandler>();
                        accountHandler.SubmitAccount("UsernameLogin", "PasswordLogin", ml, false);
                        break;

                    case "cancel":
                        menuHandler.GoUpMenu();
                        break;


                    default:
                        break;

                }
            }

            if (Input.GetButtonDown("Fire2") && !(MenuPrefabHandler.statusBoxOpened || MenuPrefabHandler.modalBoxOpened))
            { 
                menuHandler.GoUpMenu();

            }
        }
        direction = new Vector2(0, 0);
	}

    private Vector2 getDirection()
    { //if two buttons are pressed, it will only return the first occuring value rather than average them out
        Vector2 vecToReturn = new Vector2(0, 0);
        if (Input.GetKeyDown("up")) //up
        {
            vecToReturn = vecToReturn + Vector2.up;
        }
        if (Input.GetKeyDown("down")) // down
        {
            vecToReturn = vecToReturn + Vector2.down;
        }
        if (Input.GetKeyDown("right")) //right
        {
            vecToReturn = vecToReturn + Vector2.right;
        }
        if (Input.GetKeyDown("left")) //left
        {
            vecToReturn = vecToReturn + Vector2.left;
        }
        return vecToReturn;

    }
}
