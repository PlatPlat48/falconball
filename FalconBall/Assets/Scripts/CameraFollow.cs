using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public Rigidbody targetRigidbody;
	private float rotationSpeed, manualRotateSpeed, radius = 3f;
	private Vector3 angle;

	// Use this for initialization
	void Start () {
		Vector3 camOffset = new Vector3(0,1,-2).normalized;
		transform.position = camOffset*radius + target.position;
		angle = (transform.position-target.position);

		targetRigidbody.inertiaTensor = Vector3.one * .2f;
	}

	// Update is called once per frame
	void Update () {
		Vector3 mousePos = Input.mousePosition;
		mousePos.x /= Screen.width;
		mousePos.x -= .5f;
		mousePos.x *= 2;
		mousePos.y /= Screen.height;
		mousePos.y -= .5f;
		mousePos.y *= 2;
		mousePos.x = Mathf.Clamp(mousePos.x, -1f, 1f); // prevent out-of-range values
		mousePos.y = Mathf.Clamp(mousePos.y, -1f, 1f);
		mousePos.x = Mathf.Sign(mousePos.x)*Mathf.Pow(mousePos.x,2); // ease curve
		mousePos.y = Mathf.Sign(mousePos.y)*Mathf.Pow(mousePos.y,2);
		//////////

		// sit 3 units away from the target
		transform.position = target.position + angle*radius;

		// realign final position for the camera
		Vector3 targetPosition = new Vector3(transform.position.x, target.position.y+.5f, transform.position.z);
		Vector3.MoveTowards(target.position, targetPosition, Time.deltaTime);
		transform.LookAt(target);

		// orbit camera
		transform.RotateAround(target.position, Vector3.up, (rotationSpeed+manualRotateSpeed)*12*Time.deltaTime);

		angle = (transform.position-target.position).normalized;
		updateRotationSpeed();

		// scene tilting effect
		float rotY = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;
		float tiltX =  mousePos.x*Mathf.Cos(rotY) + mousePos.y*Mathf.Sin(rotY);
		float tiltY = -mousePos.x*Mathf.Sin(rotY) + mousePos.y*Mathf.Cos(rotY);
		transform.RotateAround(target.position, new Vector3(1,0,0),-tiltY*20);
		transform.RotateAround(target.position, new Vector3(0,0,1), tiltX*20);

		transform.Translate(0,.5f,0);
	}
	private void updateRotationSpeed() {
		// dampen speed
		rotationSpeed *= 1-6f*Time.deltaTime;
		manualRotateSpeed *= 1-7f*Time.deltaTime;


		// manual camera panning
		if (Input.GetKey(KeyCode.A))
			manualRotateSpeed -= Time.deltaTime * 80;
		else if (Input.GetKey(KeyCode.D))
			manualRotateSpeed += Time.deltaTime * 80;
		else {
			// only auto-rotate if nod manual rotation
			Vector3 vel = targetRigidbody.velocity;
			vel.Scale(new Vector3(1,0,1));
			float speed = vel.magnitude;
			vel.Normalize();
			Vector3 cam = angle;
			cam.Scale(new Vector3(1,0,1));
			cam.Normalize();

			float sign = -1;
			Vector3 cross = Vector3.Cross(cam,vel);
			if (cross.y < 0) 
				sign = 1;
			float dot = 1-Mathf.Abs(Vector3.Dot(vel, cam));
			rotationSpeed += Time.deltaTime * (dot*dot) * Mathf.Min(1,speed) * sign * 40;
		}
	}
}
