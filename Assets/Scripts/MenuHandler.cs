using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    private GameObject previousMenu;

    public void SetPrevious(GameObject activeMenu) {
        previousMenu = activeMenu;
    }

    public GameObject GetPrevious() {
        return previousMenu;
    }
}