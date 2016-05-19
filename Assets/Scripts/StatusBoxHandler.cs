using UnityEngine;
using System.Collections;



public class StatusBoxHandler : MonoBehaviour {
    private GameObject parent;
    private bool finishedConnection;

    // Use this for initialization
    void Start () {
        parent = gameObject.transform.parent.gameObject;

    }

    void Update() {

        if (Input.GetButtonDown("Fire1") && finishedConnection)
        {
            Destroy(parent);

        }

        

    }

    public void SetFinished(bool enabled) {
        finishedConnection = enabled;
    }
}
