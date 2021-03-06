﻿using UnityEngine;

public class HeroCamera : MonoBehaviour {
	public GameObject TargetLookAt;

	public Vector3 playerLookAtOffset = new Vector3(0,3,0);

	private Vector3 currentLookAt;
	
	private float distance = 1.2f;
	public float DistanceMin = 1.2f;
	public float DistanceMax = 4.0f;
	
	private float mouseX = 0.0f;
	private float mouseY = 0.0f;
	private float startingDistance = 0.0f;    
	private float desiredDistance = 0.0f;
	
	public float X_MouseSensitivity = 5.0f;
	public float Y_MouseSensitivity = 5.0f;
	public float MouseWheelSensitivity = 5.0f;
	public float Y_MinLimit = -40.0f;
	public float Y_MaxLimit = 80.0f;

	public float X_MaxOnFree = 30.0f;
	public float X_MaxOnBattle = 15.0f;

	private float X_Angle = 0.0f;
	private float Y_Angle = 0.0f;
	
	public  float DistanceSmooth = 0.05f;    
	private float velocityDistance = 0.0f;    
	private Vector3 desiredPosition = Vector3.zero;
	
	public float X_Smooth = 0.05f;
	public float Y_Smooth = 0.1f;

    private CharacterMovement charMovement;
    private Transform charTransform;

	void Start()
	{
        charMovement = TargetLookAt.GetComponent<CharacterMovement>();
        charTransform = TargetLookAt.transform;
		distance = Mathf.Clamp(distance, DistanceMin, DistanceMax);
		startingDistance = distance;

		Cursor.lockState = CursorLockMode.Locked;

		Reset();
	}
	
	void LateUpdate()
	{
		if (TargetLookAt == null)
			return;
		
		HandlePlayerInput();
		
		CalculateDesiredPosition();
		
		UpdatePosition();
	}
	
	void HandlePlayerInput()
	{
		float deadZone = 0.01f; // mousewheel deadZone

		if (Input.GetKeyDown("escape"))
		{
			if (Cursor.lockState == CursorLockMode.Locked){
				Cursor.lockState = CursorLockMode.None;
			}else{
				Cursor.lockState = CursorLockMode.Locked;
			}
			// Cursor.visible = (CursorLockMode.Locked == Cursor.lockState);
		}
		
		mouseX = Input.GetAxis("Mouse X") * X_MouseSensitivity;
		mouseY = Input.GetAxis("Mouse Y") * Y_MouseSensitivity;

		if ( charMovement.getMovementMode() == MovementMode.BATTLE )
			mouseX = Mathf.Clamp(mouseX, -X_MaxOnBattle, X_MaxOnBattle);
		else //if (TargetLookAt.getMovementState () != PlayerMovement.MovementMode.RUN)
			mouseX = Mathf.Clamp(mouseX, -X_MaxOnFree, X_MaxOnFree);
		
		// this is where the mouseY is limited - Helper script		
		X_Angle += mouseX;
		Y_Angle -= mouseY;
		Y_Angle = ClampAngle(Y_Angle, Y_MinLimit, Y_MaxLimit);
		
		// get Mouse Wheel Input
		float wheel = Input.GetAxis ("Mouse ScrollWheel");
		if (wheel < -deadZone || wheel > deadZone)
		{
			desiredDistance = Mathf.Clamp(distance - (wheel * MouseWheelSensitivity), 
			                              DistanceMin, DistanceMax);
		}
	}
	
	void CalculateDesiredPosition()
	{
        if (charMovement.getMovementMode() != MovementMode.RUN)
		{
			// Evaluate distance
			distance = Mathf.SmoothDamp (distance, desiredDistance, ref velocityDistance, DistanceSmooth);
			desiredPosition = CalculatePosition(Y_Angle, X_Angle, distance);
		}
		else // if (TargetLookAt.getMovementState () == PlayerMovement.MovementMode.RUN)
		{
            X_Angle = ClampAngle(X_Angle, charTransform.rotation.eulerAngles.y - 20, charTransform.rotation.eulerAngles.y + 20);

			// Evaluate distance
			distance = Mathf.SmoothDamp (distance, desiredDistance, ref velocityDistance, DistanceSmooth*2);
			desiredPosition = CalculatePosition(Y_Angle, X_Angle, distance);
		}
	}
	
	
	Vector3 CalculatePosition(float rotationX, float rotationY, float dist)
	{
		Vector3 direction = new Vector3(0, 0, -dist);
		Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        currentLookAt = Vector3.Lerp(currentLookAt, charTransform.position + playerLookAtOffset, 0.2f);

		return currentLookAt + (rotation * direction);
	}
	
	void UpdatePosition()
	{		
		transform.position = Vector3.Lerp(transform.position, desiredPosition, 0.2f);;
		transform.LookAt(currentLookAt);
	}
	
	void Reset()
	{
		mouseX = 0;
		mouseY = 10;
		distance = startingDistance;
		desiredDistance = distance;

        currentLookAt = charTransform.position;
	}
	
	float ClampAngle(float angle, float min, float max)
	{
		while (angle < -360 || angle > 360)
		{
			if (angle < -360)
				angle += 360;
			if (angle > 360)
				angle -= 360;
		}
		
		return Mathf.Clamp(angle, min, max);
	}
}
