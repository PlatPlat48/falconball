using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour {

	private Vector3 startPos;

	// Use this for initialization
	void Start () {
		startPos = transform.position;
	}

	// Update is called once per frame
	void Update () {
		if (transform.position.y < -10) {
			transform.position = startPos;//new Vector3(0,2,0);
			gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
			gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0,0);
		}
	}
}
