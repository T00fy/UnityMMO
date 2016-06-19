using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public GameObject statusBoxPrefab;
    public GameObject characterSelectPrefab;
    

    private GameObject previousMenu;
    private bool loginSuccessful;
    private GameObject status;
    private Text statusTextObj;
    private string statusText;
    private bool boxOpened;
    private GameObject cursor;
    private bool finishedConnection;

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

    public void OpenStatusBox() {
        status = Instantiate(statusBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        statusTextObj = status.GetComponentInChildren<Text>();
        boxOpened = true;
    }

    public void SetDestroyStatusBox() {
        finishedConnection = true;

    }

    public void SetLoginSuccessful(bool b) {
        loginSuccessful = b;
        if (loginSuccessful == true) {
            DisplayCharacterScreen();
        }
    }

    public void SetCursor(GameObject cursor)
    {
        this.cursor = cursor;
    }

    public void ToggleCursor(bool enabled)
    {
        cursor.SetActive(enabled);

    }

    private void DisplayCharacterScreen() {
        

    }

    public void SetStatusText(string statusText)
    {
        this.statusText = statusText;
    }


    void Update()
    {
        if (Input.GetButtonDown("Fire1") && finishedConnection && status != null)
        {
            Destroy(status);

        }

        if (statusTextObj != null)
        {
            statusTextObj.text = "Status: " + statusText;
        }

        if (boxOpened && status == null)
        {
            ToggleCursor(true);

        }



    }


}