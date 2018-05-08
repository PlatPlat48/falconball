using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour {

	public FalconIO falconIO;
	public Transform targetCamera;
	private float gravity = 12f, rotY;
	private Vector3 lastVelocity, delta;

	// Use this for initialization
	void Start () {
		//lastVelocity = gameObject.GetComponent<Rigidbody>().velocity;
	}

	// Update is called once per frame
	void Update () {
		Vector3 mousePos = Input.mousePosition;
		mousePos.x /= Screen.width;
		mousePos.x -= .5f;
		mousePos.x *= 3;
		mousePos.y /= Screen.height;
		mousePos.y -= .5f;
		mousePos.y *= 3;
		mousePos.x = Mathf.Clamp(mousePos.x, -1f, 1f); // prevent out-of-range values
		mousePos.y = Mathf.Clamp(mousePos.y, -1f, 1f);
		mousePos.x = Mathf.Sign(mousePos.x)*Mathf.Pow(mousePos.x,2); // ease curve
		mousePos.y = Mathf.Sign(mousePos.y)*Mathf.Pow(mousePos.y,2);
		//////////

        // create the gravity vector based on current mouse position
		Vector3 gravDirection = new Vector3(0,-2,0);
		rotY = Mathf.Deg2Rad * targetCamera.rotation.eulerAngles.y;
		gravDirection.x = mousePos.x*Mathf.Cos(rotY) + mousePos.y*Mathf.Sin(rotY);
		gravDirection.z = -mousePos.x*Mathf.Sin(rotY) + mousePos.y*Mathf.Cos(rotY);
		gravDirection.Normalize();
        // scale appropriately
        gravDirection.Scale(
			new Vector3(
				gravity*Time.deltaTime,
				gravity*Time.deltaTime,
				gravity*Time.deltaTime
			));
        // add gravity vector to velocity
		Vector3 vel = gameObject.GetComponent<Rigidbody>().velocity;
		gameObject.GetComponent<Rigidbody>().velocity = vel + gravDirection;

        // record delta in velocity vector
        // divide by deltaTime to get velocity change per-second, not per-frame
        delta = (lastVelocity - vel)/Time.deltaTime;
        lastVelocity = vel;

        //if (delta.magnitude > .25 && delta.magnitude < 5) {
            /*
             * Force axes:
             * 0 = falcon x, positive is left
             * 1 = falcon y, positive is down
             * 2 = falcon up/down, positive is up
             */
            float xVel = delta.x * Mathf.Cos(rotY) - delta.z * Mathf.Sin(rotY);
            float zVel = delta.x * Mathf.Sin(rotY) + delta.z * Mathf.Cos(rotY);
            
            string s = "d:"+ -xVel +","+ -zVel +","+ delta.y;
			//Debug.Log("Delta: "+s);
            //falconIO.messageStream.Write(s);
            //falconIO.messageStream.Flush();
        //} //else
            //Debug.Log("Skipping delta: " + delta);
	}

    void OnCollisionEnter(Collision c)
    {
        if (delta.magnitude < .25)
            return;
    
        // force is how forcefully we will push the player
        // currently just based on current velocity
        Vector3 vel = gameObject.GetComponent<Rigidbody>().velocity;
        float force = vel.magnitude;

        //if (force < .5)
        //    return;

        // normal between collision point and player center of mass
        Vector3 dir = transform.position - c.contacts[0].point;
        // flip the vector, normalize, then scale by force
        dir = dir.normalized * force;

        // rotate appropriately based on camera angle
        float xVel = dir.x * Mathf.Cos(rotY) - dir.z * Mathf.Sin(rotY);
        float zVel = dir.x * Mathf.Sin(rotY) + dir.z * Mathf.Cos(rotY);

        string s = "f:"+ -xVel +","+ -zVel +","+ dir.y;
        Debug.Log("Force: "+s);
        falconIO.messageStream.Write(s);
        falconIO.messageStream.Flush();
    }
}
