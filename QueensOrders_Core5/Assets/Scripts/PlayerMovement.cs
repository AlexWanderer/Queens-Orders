﻿using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	enum MoveMode{FREE, BATTLE, RUN};

	// Camera
	public Transform myCamera;				// Used for 3rd person movement

	// Curves
	public AnimationCurve angleAccelFactor;	// Lower speed on high hills

	// Movement 
	public float accelFree = 20.0f;
	public float accelRun = 35.0f;
	public float jumpFree = 8.0f;
	public float jumpBattle = 15.0f;

	public float gravity = 9.81f;
	public float frictionGroundMoving = 5.0f;
	public float frictionGroundNMoving = 10.0f;
	public float frictionAir = 0.01f;

	private Vector3 acceleration = Vector3.zero;
	private Vector3 velocity = Vector3.zero;

	// Private members
	private float currentAccel;
	private CharacterController controller;
	private CapsuleCollider body;

	private MoveMode movementMode;
	private const float h = 0.01666666f;

	private Vector3 inputDirection;

	public void Start ()
	{
		movementMode = MoveMode.FREE;
		controller = GetComponent<CharacterController> ();
		body = GetComponent<CapsuleCollider> ();

		currentAccel = accelFree;
	}

	private Vector3 FindGroundNormal()
	{
		RaycastHit hit;
		if (Physics.Raycast (transform.position, -Vector3.up, out hit, body.height + 5.0f)) {
			return hit.normal.normalized;
		}

		return Vector3.up*0.01f; // weak normal
	}

	protected void Jump(Vector3 groundNormal)
	{
		if (!controller.isGrounded)
			return;

		if ( movementMode ==  MoveMode.FREE){ // FREE
			velocity = groundNormal*jumpFree;
			acceleration.y = 0;
		} else if (movementMode == MoveMode.RUN ) { // BATTLE
			if (inputDirection.magnitude <= 0)
				return;
			velocity.y = 0;
			Vector3 w = inputDirection + groundNormal;
			w.Normalize();
			w.x *= w.y;
			w.z *= w.y;
			w.y *= 0.5f;
			velocity = w.normalized * jumpBattle;
			acceleration = Vector3.zero;
		}
	}

	private void UpdateModeNormal()
	{
		Vector3 friction = Vector3.zero;

		HandleInput();
		Vector3 groundNormal = FindGroundNormal();
		
		if (controller.isGrounded)
		{
			if (inputDirection.magnitude > 0)
			{
				friction = velocity*frictionGroundMoving;
				friction.y = 0;
			} else if (velocity.magnitude < 1){
				velocity = Vector3.zero;
			} else {
				friction = velocity*frictionGroundNMoving;
				friction.y = 0;
			}
			
			// Use ground direction
			Vector3 mm = Vector3.ProjectOnPlane(inputDirection, groundNormal);
			float angle = Vector3.Angle(mm, inputDirection);
			
			// Going up slopes makes you go slower
			if (mm.y >= 0) {
				acceleration = mm * angleAccelFactor.Evaluate(angle) * currentAccel;
			} else {
				acceleration = mm * currentAccel;
			}
			acceleration.y -= gravity;
			

			velocity.y = -gravity;
			// Colocar isso dentro de HandleInput?
			if (Input.GetButton("Jump"))
			{
				Jump(groundNormal);
			}

		} else {
			acceleration.y -= gravity;
			friction = velocity*frictionAir;
			friction.y = 0;
		}

		Vector3 DO = transform.position+new Vector3(0, 1.5f, 0); // debug offset
		Debug.DrawLine (DO, DO - friction, Color.red);
		Debug.DrawLine (DO, DO + acceleration, Color.blue);
		Debug.DrawLine (DO, DO + velocity, Color.green);

		// Apply friction
		velocity = velocity + (acceleration-friction) * h;
//		Vector3 newvelXZ = new Vector3(newvel.x, 0, newvel.z);
//		Vector3 newvelXZbefore = new Vector3(velocity.x, 0, velocity.z);
//		if (controller.isGrounded && newvelXZ.magnitude > currentMaxSpeed && newvelXZbefore.magnitude < newvelXZ.magnitude){
//			velocity.x = velocity.x; // Do not sum if change exceeds max speed
//			velocity.z = velocity.z;
//		} else {
//			velocity.x = newvel.x;
//			velocity.z = newvel.z;
//		}
//		velocity.y = newvel.y;

		//Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
		//print (controller.isGrounded+" Normal: "+groundNormal + " - Accel: " + acceleration.magnitude + " VelocityXZ: " + velocityXZ + velocityXZ.magnitude + " velocityY: "+ velocity.y + " Friction: "+ friction.magnitude);
		
		// Move
		controller.Move(velocity * h);
		
		// Rotate - Character will look towards it's moving velocity
		this.transform.LookAt(this.transform.position + new Vector3 (velocity.x, 0.0f, velocity.z));
	}

	public void Update()
	{
		UpdateModeNormal();
	}

	private void HandleInput ()
	{
		// print ("Vertical: "+Input.GetAxis("Vertical"));
		if (Input.GetAxis ("Run") > 0){
			currentAccel = accelRun;
			movementMode = MoveMode.RUN;
		}else{
			currentAccel = accelFree;
			movementMode = MoveMode.FREE;
		}

		// Movement direction
		Vector3 moveX = myCamera.forward * Input.GetAxis ("Vertical");
		Vector3 moveY = myCamera.right * Input.GetAxis ("Horizontal");

		Vector3 move = moveX + moveY;
		move.y = 0;
		moveX.Normalize ();
		inputDirection = move.normalized;
	}

}