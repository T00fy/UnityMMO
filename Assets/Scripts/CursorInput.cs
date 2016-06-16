using UnityEngine;
using System;
using UnityEngine.UI;

public class CursorInput : MonoBehaviour {
    
    public MenuHandler previousMenu;

    private GameObject cursor;
    private GameObject activeMenu;
    private bool enteringText;
    private InputField inputField;
    private CursorMover cm;
    private GameObject selectedOption;

    // Use this for initialization

    void Awake () {
        cursor = gameObject;
        string parent = cursor.transform.parent.name;
        activeMenu = GameObject.Find(parent);
        cm = gameObject.GetComponent<CursorMover>();
        selectedOption = cm.GetSelectedOption();
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
        string type = "";
        MenuLink ml = selectedOption.GetComponent<MenuLink>();

        if (ml != null) {
            type = ml.GetState().ToString();
        }

        if (type == "inputfield")
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

                switch (type) {

                    case "menu":
                        previousMenu.SetPrevious(activeMenu);
                        GameObject enterMenu = ml.GetMenuItem();
                        activeMenu.SetActive(false);
                        enterMenu.SetActive(true);
                        break;

                    case "register":
                        LoginOrRegister("UsernameRegister", "PasswordRegister", "register", ml);
                        break;

                    case "login":
                        LoginOrRegister("UsernameLogin", "PasswordLogin", "login", ml);
                        break;


                    default:
                        break;

                }
            }

            if (Input.GetButtonDown("Fire2"))
            {

                //    enterMenu.SetActive(true);
                if (previousMenu.GetPrevious() != null)
                {
                    activeMenu.SetActive(false);
                    previousMenu.GetPrevious().SetActive(true);

                }

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

    private void LoginOrRegister(string findUser, string findPass, string cmdString, MenuLink ml)
    {
        GameObject passwordGameObj = GameObject.Find(findPass);
        InputField passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find(findUser);
        InputField usernameInput = userGameObj.GetComponent<InputField>();

        GameObject subObj = ml.GetMenuItem();
        var packetProcessor = subObj.GetComponent<PacketProcessor>();
        string password = passwordInput.text;
        string userName = usernameInput.text;
        string cmd = cmdString + " " + userName + " " + password;

        packetProcessor.LoginOrRegister(userName, password);
    }
}
