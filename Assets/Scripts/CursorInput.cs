using UnityEngine;
using System;
using UnityEngine.UI;

public class CursorInput : MonoBehaviour {

    public GameObject selectedOption;
    public GameObject[] menuObjects;
    public MenuHandler previousMenu;

    private GameObject cursor;
    private GameObject activeMenu;
    private int counter;
    private float scaleOfCanvas;
    private bool enteringText;
    private InputField inputField;
	// Use this for initialization
	void Awake () {
        cursor = GameObject.Find("Cursor");
        string parent = cursor.transform.parent.name;
        activeMenu = GameObject.Find(parent);
        scaleOfCanvas = GameObject.Find("MainMenu").transform.localScale.x;
        counter = 0;

    }
	
	void Update () {
        if (Input.GetButtonDown("Vertical"))
        {
            enteringText = false;
            DoVerticalInputs();

        }
        if (Input.GetButtonDown("Horizontal"))
        {
            enteringText = false;
            DoHorizontalInputs();
        }

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
            if (inputField != null) {
                inputField.enabled = false;
            }
            
        }

        if (!enteringText) {

            if (Input.GetButtonDown("Fire1"))
            {


                if (type == "menu")
                {
                    
                    previousMenu.SetPrevious(activeMenu);
                    GameObject enterMenu = ml.GetMenuItem();

                    activeMenu.SetActive(false);
                    enterMenu.SetActive(true);
                }

                if (type == "register")
                {
                    GameObject passwordGameObj = GameObject.Find("PasswordRegister");
                    InputField passwordInput = passwordGameObj.GetComponent<InputField>();

                    GameObject userGameObj = GameObject.Find("UsernameRegister");
                    InputField usernameInput = userGameObj.GetComponent<InputField>();

                    GameObject submit = ml.GetMenuItem();
                    var register = submit.GetComponent<ClickRegister>();
                    string password = passwordInput.text;
                    string userName = usernameInput.text;
                    string cmd = "register " + userName + " " + password;

                    register.StartConnection(cmd, userName, password);

                }

                if (type == "login")
                {
                    GameObject passwordGameObj = GameObject.Find("PasswordLogin");
                    InputField passwordInput = passwordGameObj.GetComponent<InputField>();

                    GameObject userGameObj = GameObject.Find("UsernameLogin");
                    InputField usernameInput = userGameObj.GetComponent<InputField>();

                    GameObject submit = ml.GetMenuItem();
                    var login = submit.GetComponent<ClickRegister>();
                    string password = passwordInput.text;
                    string userName = usernameInput.text;
                    string cmd = "login " + userName + " " + password;

                    login.StartConnection(cmd, userName, password);

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


	}

    private void DoHorizontalInputs()
    {
        var direction = getDirection();
        //-1 is right
        // 1 is left
        float current = menuObjects[counter].transform.position.y;
        float next = 0.0f;
        try
        {
            next = menuObjects[counter + direction].transform.position.y;

            if (!sameYPositions(next, current))
            { //hack to stop further execution of block
                throw new IndexOutOfRangeException();
            }
            counter = counter + direction;

            if (sameYPositions(next, current))
            {

                cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            }
        }
        catch (IndexOutOfRangeException)
        {
            //stay in current position, no valid input
            next = current;

            if (counter < 0)
            {
                counter = 0;
            }
            if (counter > menuObjects.Length - 1)
            {
                counter = menuObjects.Length;
            }
            return;
        }
        selectedOption = menuObjects[counter];
    }

    private void DoVerticalInputs()
    {
        var direction = getDirection();
        //-1 is up
        // 1 is down
        float current = menuObjects[counter].transform.position.y;
        float next = 0.0f;
        try
        {
            next = menuObjects[counter + direction].transform.position.y;

            if (sameYPositions(next, current))
            {
                counter = GetNextVerticalSelection(direction, current);
                cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            }
            else {
                counter = counter + direction;
                cursor.transform.position = new Vector3(GetCursorXPosition(menuObjects[counter]), menuObjects[counter].transform.position.y);
            }
        }
        catch (IndexOutOfRangeException)
        {
            //stay in current position, no valid input
            next = current;

            if (counter < 0)
            {
                counter = 0;
            }
            if (counter > menuObjects.Length - 1)
            {
                counter = menuObjects.Length - 1;
            }
            return;
        }
        selectedOption = menuObjects[counter];
    }

    private int GetNextVerticalSelection(int direction, float current)
    {
        for (int i = counter; i < menuObjects.Length; i = i + direction) {
                if (!sameYPositions(current, menuObjects[i].transform.position.y))
                {
                    return i;
                }
        }
        throw new IndexOutOfRangeException();
    }

    private bool sameYPositions(float pos1, float pos2)
    {
        if (Mathf.Approximately(pos1, pos2)) {
            return true;
        }
        return false;
    }

    private float GetCursorXPosition(GameObject selected)
    {
        RectTransform rt = selected.GetComponent<RectTransform>();
        float x = selected.transform.position.x/scaleOfCanvas;
        float cursorPos = x -( rt.rect.width / 2.0f);
        cursorPos = cursorPos - 40.0f;

        //have to multiply by the scale set in canvas
        cursorPos = cursorPos * scaleOfCanvas;

        
        return cursorPos;
    }

    private int getDirection()
    {
        if (Input.GetAxis("Vertical") > 0)
        {
            return -1;
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            return 1;
        }
        if (Input.GetAxis("Horizontal") > 0)
        {
            return 1;
        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            return -1;
        }
        return 1;
    }
}
