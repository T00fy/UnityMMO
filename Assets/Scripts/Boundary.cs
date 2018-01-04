using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour {

    public GameObject player;
	// Update is called once per frame
	void Update () {
        var rb = player.GetComponent<Rigidbody2D>();
        rb.position = new Vector2(Mathf.Clamp(rb.position.x, -22.0f, 16.0f), Mathf.Clamp(rb.position.y, -8.0f, 8.0f));
	}
}
