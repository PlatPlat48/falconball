using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour {

    private bool collected;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (collected) {
            float s = 1 - 5*Time.deltaTime;
            transform.localScale = transform.localScale * s;
            //if (transform.localScale.magnitude < .1)
            //    Destroy(gameObject);
            if (Input.GetKeyDown(KeyCode.R)) {
                collected = false;
                transform.localScale = new Vector3(1,1,1);
            }

        }
        transform.Rotate(50*Time.deltaTime, 250*Time.deltaTime, 50*Time.deltaTime);
	}

    void OnTriggerEnter(Collider other)
    {
        collected = true;
        /*
        if (other.tag == "Coin")
        {
            score += 10;
            Destroy(gameObject); // Or whatever way you want to remove the coin.
        }
        */
    }
}
