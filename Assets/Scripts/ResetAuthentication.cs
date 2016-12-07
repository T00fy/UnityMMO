using UnityEngine;
using System.Collections;

public class ResetAuthentication : MonoBehaviour {

	void Update () {
        PacketProcessor.isAuthenticated = false;
	}
}
