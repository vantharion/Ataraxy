﻿using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Character/Controller")]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(MouseView))]
[RequireComponent(typeof(Croucher))]
[RequireComponent(typeof(Rigidbody))]

/*

 * Public functions

 * Footstep();
 * IsGrounded();
 * IsMoving();
 * CanStand();
 * Jump();

*/
public class Controller : MonoBehaviour {

	public bool controllerAble = true;
	public Transform mCamera;    //The root object that contains the camera
	public float checksPerSecond = 10f; //The amount of times per second to run IsGrounded(), CanStand() and IsMoving()

	[Header("Falling")]
	public float fallThreshold = 3f;    //How many units to fall to start counting as damage
	public float fallDamageMultiplier = 3f;
	public float maxFallSpeed = 20f;

	Croucher croucher;
	CapsuleCollider capsule;

	[Header("Footsteps")]
	public AudioSource audioSource;
	public AudioClip footstepSound;
	public float footstepSpeed = 1.5f;

	[Header("Speed")]
	public float currentSpeed;
	[Space(10f)]
	public float walkSpeed = 5f;
	public float crouchSpeed = 2.5f;
	public float climbSpeed = 5.5f;
	public float runSpeed = 7.5f;

	[Header("Running")]
	public bool ableToRun = false;
	public int stamina = 100;
	public float staminaRegenPerSecond = 15f;
	float nextStamina;
	[HideInInspector]
	public int maxStamina = 100;

	[Header("Jumping")]
	public bool canJump = true;
	public float jumpHeight = 2f;   //in unity units
	public int maxJumps = 2;
	public float airAccelerator = 0.5f;
	public float groundAccelerator = 1.5f;

	//Private variables
	#region Player Location
	//[HideInInspector]
	public Island lastLocation;
	#endregion
	bool grounded;
	float nextCheck;
	bool canStand;
	float speed;
	float acceleration;
	int jumpsDone = 0;
	float jumpedYPos;
	float landedYPos;
	bool lastGrounded;
	bool moving;
	bool lastCrouching;
	float nextFootstep;
	Rigidbody myRB;
	float lastAiredPos;
	Vector3 velocityChange;

	//Hidden variables
	//[HideInInspector]
	public bool crouching;
	//This can be used to alter the speed from another script
	//[HideInInspector]
	public float speedMultiplier = 1f;

	void Start ()
	{
		croucher = GetComponent<Croucher>();
		capsule = GetComponent<CapsuleCollider>();

		myRB = GetComponent<Rigidbody>();
		myRB.freezeRotation = true;

		maxStamina = stamina;

		grounded = IsGrounded();
		lastGrounded = grounded;

		//If this is networked, make sure that the rigidbody is kinematic is true for the
		//people with a !networkView.isMine (myRB.isKinematic = true;)
	}
	public void Jump()
	{
		if(!canJump)
		{
			return;
		}
		if(grounded)
		{
			//Normal jumping if the player can stand
			if(canStand)
			{
				jumpsDone = 0;
				lastAiredPos = transform.position.y;
				myRB.velocity = new Vector3(myRB.velocity.x, CalculateJumpVerticalSpeed(), myRB.velocity.z);
			}
		}
		else
		{
			//Double jumping in mid air if possible
			if(jumpsDone < maxJumps-1)
			{
				jumpsDone++;
				lastAiredPos = transform.position.y;
				myRB.velocity = new Vector3(myRB.velocity.x, CalculateJumpVerticalSpeed(), myRB.velocity.z);
			}
		}
	}
	void Update()
	{
		#region Time Checking for Fixed Update
		if (Time.time > nextCheck)
		{
			nextCheck = Time.time + (1f / checksPerSecond);
			grounded = IsGrounded();
			moving = IsMoving();
			canStand = CanStand();
		}
		#endregion

		#region Ground to Air Transitions
		if (lastGrounded != grounded)
		{
			#region Landing or Jumping (for Audio)
			//This sound will play when jumping or when landing
			if (grounded)
			{
				Footstep();
				nextFootstep = Time.time + (0.2f);
			}
			else
			{
				if (nextFootstep < Time.time - (0.05f))
				{
					Footstep();
				}
			}
			#endregion
			#region Landing (for fall damage & jump count resetting)
			lastGrounded = grounded;
			if (lastGrounded == true)
			{
				landedYPos = transform.position.y;
				if (jumpedYPos > landedYPos)
				{
					float distanceFell = jumpedYPos - landedYPos;
					if (distanceFell > fallThreshold)
					{
						if (distanceFell * fallDamageMultiplier > 1.5f)
						{
							distanceFell -= fallThreshold;
							//Here is where you will do your fall damage calculation
							//playerHealth -= Mathf.RoundToInt(distanceFell * fallDamageMultiplier);
						}
					}
				}
			}
			else
			{
				lastAiredPos = transform.position.y;
				jumpsDone = 0;
			}
			#endregion
		}
		#endregion
		#region Airborn Handling
		if (!grounded)
		{
			if(transform.position.y > lastAiredPos)
			{
				lastAiredPos = transform.position.y;
			}
			else
			{
				jumpedYPos = lastAiredPos;
			}
		}
		#endregion
		//Stamina regeneration for running
		if (ableToRun)
		{
			if (stamina < maxStamina && Time.time > nextStamina)
			{
				nextStamina = Time.time + (1f / staminaRegenPerSecond);
				stamina += 1;
			}
		}
		//Footstep sounds when moving
		if (moving && Time.time > nextFootstep && grounded)
		{
			float mp = Random.Range(0.8f, 1.2f);
			nextFootstep = Time.time + ((3.5f / currentSpeed) * mp) / footstepSpeed;
			Footstep();
		}

		//Where the jump function happens
		if (Input.GetButtonDown("Jump") && controllerAble)
		{
			Jump();
		}
	}
	public void Footstep()
	{
		//networkView.RPC("Step", RPCMode.All);
		Step();
	}
	[RPC]
	void Step()
	{
		if(audioSource && footstepSound)
		{
			audioSource.pitch = Random.Range(0.9f, 1.1f);
			audioSource.volume = 0.8f;
			audioSource.maxDistance = 15f;
			audioSource.PlayOneShot(footstepSound);
		}
	}
	[RPC]
	void CrouchState(bool newCrouch)
	{
		croucher.crouching = newCrouch;
	}
	void FixedUpdate ()
	{
		currentSpeed = Mathf.Round(myRB.velocity.magnitude);
		speed = walkSpeed;

		if(lastCrouching != crouching)
		{
			lastCrouching = crouching;
			//This can be an rpc call
			//networkView.RPC("CrouchState", RPCMode.All, crouching);
			CrouchState(crouching);
		}

		#region Disabled Crouching
		/*if(Input.GetKey(KeyCode.LeftControl) || !canStand)
		{
			if (grounded)
			{
				speed = crouchSpeed * speedMultiplier;
			}

			crouching = true;
		}
		else */
		#endregion
		#region Running
		if (Input.GetKey(KeyCode.RightShift) && canStand && stamina > 0 && ableToRun)
		{
			//Running
			if (grounded)
			{
				stamina -= 1;
				speed = runSpeed * speedMultiplier;
			}
			else
			{
				speed = walkSpeed * speedMultiplier;
			}
			crouching = false;
		}
		else
		{
			speed = walkSpeed * speedMultiplier;

			crouching = false;
			
			#region Disabled Crouching
			/*
			if(!canStand)
			{
				if (grounded)
				{
					speed = crouchSpeed * speedMultiplier;
				}
				crouching = true;
			}
			else
			{
				speed = walkSpeed * speedMultiplier;
				
				crouching = false;
			}*/
			#endregion
		}
		#endregion

		if (grounded)
		{
			if(controllerAble)
			{
				acceleration = groundAccelerator;
			}
		}
		else
		{
			if(controllerAble)
			{
				acceleration = airAccelerator;
			}
			else
			{
				acceleration = 0.1f;
			}
		}

		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
		//Debug.Log(input + "\n");
		Vector3 targetVelocity = input;
		targetVelocity = transform.TransformDirection(targetVelocity.normalized) * speed;

		velocityChange = (targetVelocity - myRB.velocity);
		velocityChange.x = Mathf.Clamp(velocityChange.x, -acceleration * 2, acceleration * 2);
		velocityChange.z = Mathf.Clamp(velocityChange.z, -acceleration * 2, acceleration * 2);
		velocityChange.y = 0f;
		
		//If VelocityChange is normalized, the degree of air control we have goes WAY down but we can achieve the appropriate movement in directions.
		//velocityChange.Normalize();

		//Debug.DrawLine(transform.position, transform.position + targetVelocity, Color.magenta, .05f);
		//Debug.DrawLine(transform.position + (Vector3.up * 1.2f) + myRB.velocity, transform.position + (Vector3.up * 1.2f) + myRB.velocity + velocityChange, Color.yellow, .05f);
		//Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + myRB.velocity, Color.red, .05f);

		//Debug.Log("VelCh: " + velocityChange.ToString() + "\tVC Mag: " + velocityChange.magnitude + "\tSpeed: " + speed + "\tAccel: " + acceleration
		//	+ "\nInput: " + input.ToString() + "\tIn Mag: " + input.magnitude + "\n\n");

		if (controllerAble)
		{
			//Debug.Log("Walking Normal\n" + targetVelocity.sqrMagnitude + "\t" + (speed * speed));

			//Speed limit for diagonal walking
			if (targetVelocity.sqrMagnitude > (speed * speed))
			{
			//	Debug.Log("Walking Diagonal\n" + targetVelocity.sqrMagnitude + "\t" + (speed * speed));
				targetVelocity = targetVelocity.normalized * speed;
			}
			
			//Speed limit for falling
			if (myRB.velocity.y < -maxFallSpeed)
			{
				myRB.velocity = new Vector3(myRB.velocity.x, -maxFallSpeed, myRB.velocity.z);
			}
			if(grounded)
			{
				myRB.AddForce(velocityChange, ForceMode.VelocityChange);
			}
			else
			{
				//If in mid air then only change movement speed if actually trying to move
				if (input.x != 0 || input.z != 0)
				{
					if (velocityChange.magnitude > .02f)
					{
						//if (collided = null)
						//{
							myRB.AddForce(velocityChange * 05f, ForceMode.Acceleration);
						//}
					}
				}
			}
		}
		else
		{
			//If the player isnt supposed to move, the player movement on x and z axis is 0
			targetVelocity = Vector3.zero;
			myRB.velocity = new Vector3(0, myRB.velocity.y, 0);
		}

		//collided = null;
	}
	public bool IsMoving()
	{
		float minSpeed = 0.5f;
		return myRB.velocity.magnitude > minSpeed;
	}
	public bool CanStand()
	{
		//croucher.defHeight is the original height of the capsule collider
		//because when crouching the capsule collider height changes
		//divided by 2 because the cast starts from the center of the player and not the top
		float castDistance = croucher.defHeight / 2f + 0.1f;

		Vector3 centerCast = new Vector3(transform.position.x, croucher.globalYPosition, transform.position.z);
		
		return !Physics.Raycast(centerCast, transform.up, castDistance);
	}
	public bool IsGrounded()
	{
		float castRadius = capsule.radius-0.1f;
		float castDistance = capsule.height/2f+0.5f;

		//1 cast in the middle, and 4 more casts on the edges of the collider
		Vector3 leftCast = new Vector3(transform.position.x-castRadius, transform.position.y, transform.position.z);
		Vector3 rightCast = new Vector3(transform.position.x+castRadius, transform.position.y, transform.position.z);
		Vector3 frontCast = new Vector3(transform.position.x, transform.position.y, transform.position.z+castRadius);
		Vector3 backCast = new Vector3(transform.position.x, transform.position.y, transform.position.z-castRadius);
		Vector3 centerCast = transform.position;

		List<Vector3> casts = new List<Vector3>();
		casts.Add(centerCast);
		casts.Add(frontCast);
		casts.Add(backCast);
		casts.Add(leftCast);
		casts.Add(rightCast);

		RaycastHit hit;
		for (int i = 0; i < casts.Count; i++)
		{
			if (Physics.Raycast(casts[i], -transform.up, out hit, castDistance))
			{
				if (hit.collider.gameObject.tag == "Island")
				{
					lastLocation = hit.collider.gameObject.GetComponent<Island>();
				}
				return true;
			}
		}

		return false;

		/*return (Physics.Raycast(leftCast, -transform.up, castDistance) || Physics.Raycast(rightCast, -transform.up, castDistance) || 
			Physics.Raycast(frontCast, -transform.up, castDistance) || Physics.Raycast(backCast, -transform.up, castDistance) || 
				Physics.Raycast(centerCast, -transform.up, castDistance));*/
	}
	float CalculateJumpVerticalSpeed ()
	{
		return Mathf.Sqrt(jumpHeight * 20f);
	}
	void OnTriggerStay(Collider what)
	{
		if(what.name == "Ladder")
		{
			myRB.velocity = Vector3.Lerp(myRB.velocity, new Vector3(myRB.velocity.x/4f, climbSpeed, myRB.velocity.z/4f), Time.deltaTime*15f);
		}
	}
	void OnTriggerExit(Collider what)
	{
		if(what.name == "Ladder")
		{
			jumpedYPos = transform.position.y;
		}
	}
	void OnTriggerEnter(Collider what)
	{
		if(what.name == "Death")
		{
			//playerHealth = 0;
		}
		if(what.name == "Safepad")
		{
			jumpedYPos = what.transform.position.y;
		}
	}

	//GameObject collided = null;
	void OnCollisionEnter(Collision what)
	{
		//collided = what.gameObject;
		if(what.transform.name == "Safepad")
		{
			jumpedYPos = what.transform.position.y;
		}
	}
	//void OnCollisionStay(Collision what)
	//{
	//	GameObject collided = what.gameObject;
	//}
}