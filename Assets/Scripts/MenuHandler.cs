using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public GameObject statusBoxPrefab;
    public GameObject characterSelectPrefab;
    public GameObject characterMenu;
    public GameObject home;

    private GameObject activeMenu;
    private GameObject previousMenu;
    private bool loginSuccessful;
    private GameObject status;
    private Text statusTextObj;
    private string statusText;
    private bool boxOpened;
    private GameObject cursor;
    private bool finishedConnection;
    private bool statusBoxClosed;

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

    public bool StatusBoxIsClosed()
    {
        return statusBoxClosed;
    }

    public void OpenStatusBox() {
        status = Instantiate(statusBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        statusTextObj = status.GetComponentInChildren<Text>();
        boxOpened = true;
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


    /// <summary>
    /// Destroys the status box instantly, without waiting for user input
    /// </summary>
    public void DestroyStatusBox()
    {
        Destroy(status);
        statusBoxClosed = true;
    }


    void Update()
    {
        if (Input.GetButtonDown("Fire1") && finishedConnection && status != null)
        {
            Destroy(status);
            statusBoxClosed = true;
            if (loginSuccessful)
            {
                EnterMenu(characterMenu, activeMenu);
                loginSuccessful = false;
                previousMenu = home;
            }

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

    public void LoggedInSuccessfully()
    {
        loginSuccessful = true;
    }

    public void EnterMenu(GameObject enterMenu, GameObject previousMenu)
    {
        this.previousMenu = previousMenu;
        previousMenu.SetActive(false);
        enterMenu.SetActive(true);
        activeMenu = enterMenu;
    }
}