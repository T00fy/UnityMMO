using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CursorInput : MonoBehaviour {

    private GameObject cursor;
    private GameObject LoginText;
    private GameObject RegisterText;
    private GameObject SettingsText;
    private int counter;
	// Use this for initialization
	void Awake () {
        cursor = GameObject.Find("Cursor");
        LoginText = GameObject.Find("Login");
        RegisterText = GameObject.Find("Register");
        SettingsText = GameObject.Find("Settings");
        counter = 0;

    }
	
	// Update is called once per frame
	void Update () {
        GameObject[] menuObjects = new GameObject[3];
        menuObjects[0] = LoginText;
        menuObjects[1] = RegisterText;
        menuObjects[2] = SettingsText;
       
        if (Input.GetButtonDown("Vertical")) {
            var direction = getDirection();
            try
            {
                if (counter < menuObjects.Length)
                {
                    counter = counter + direction;
                    cursor.transform.position = new Vector3(cursor.transform.position.x, menuObjects[counter].transform.position.y);
                }

            }
            catch (IndexOutOfRangeException) {
                if (counter < 0) {
                    counter = 0;
                }
                else
                {
                    counter = menuObjects.Length - 1;

                }
                
            }

            
        }

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
        return 0;
    }
}
