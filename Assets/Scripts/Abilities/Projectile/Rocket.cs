﻿using UnityEngine;
using System.Collections;

public class Rocket : Projectile
{
	public GameObject target;

	public bool homing = true;
	public float explosiveDamage;

	public float homingVelocity;
	public float blastRadius;
	public float fuelRemaining;
	public Vector3 dirToTarget;
	//private bool detonateOnAnything = false;
	public Detonator explosive;
	public GameObject body;

	public AudioSource rocketThrust;

	public override void Start()
	{
		body = transform.FindChild("Rocket Body").gameObject;
		rocketThrust = AudioManager.Instance.MakeSource("Rocket_Thrust", transform.position, transform);
		rocketThrust.minDistance = 9;

		rocketThrust.loop = true;

		rocketThrust.Play();
	}

	public override void Update()
	{
		if (fuelRemaining > 0)
		{
			fuelRemaining -= Time.deltaTime;

			if (homing)
			{
				if (target == null || !target.activeInHierarchy)
				{
					if (rocketThrust != null)
					{
						rocketThrust.Stop();
					}

					fuelRemaining = 0;
				}
				else
				{
					//Update the direction we want to go.
					dirToTarget = target.transform.position - (transform.position + Time.deltaTime * rigidbody.velocity);
					dirToTarget.Normalize();

					//Apply a force in 
					rigidbody.AddForce(dirToTarget * homingVelocity * rigidbody.mass);

					//Debug.Log("Current Speed: " + rigidbody.velocity.magnitude + "\nFuel: " + fuelRemaining);
				}
			}
		}
		else
		{
			if (rocketThrust != null)
			{
				rocketThrust.Stop();
			}
			rigidbody.useGravity = true;
			rigidbody.drag = .3f;
			gameObject.particleSystem.enableEmission = false;
		}

		//Face the the homing object in the direction it is moving. This gives the illusion of turning.
		transform.LookAt(transform.position + rigidbody.velocity * 3);
	}

	public override void ProjectileHitTarget(Entity target)
	{

	}

	public override void Collide()
	{
		rigidbody.drag += 2;
		Detonator det;
		if (explosive != null)
		{
			det = (Detonator)GameObject.Instantiate(explosive, transform.position, Quaternion.identity);

			det.Explode();
		}

		if (rocketThrust != null)
		{
			rocketThrust.Stop();
		}

		AudioSource rocketAud = AudioManager.Instance.MakeSourceAtPos("Rocket_Explosion", transform.position);
		rocketAud.minDistance = 9;
		rocketAud.Play();
		
		gameObject.particleSystem.enableEmission = false;
		gameObject.collider.enabled = false;
		body.renderer.enabled = false;
		enabled = false;
		body.SetActive(false);

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, blastRadius);
		int i = 0;
		while (i < hitColliders.Length)
		{
			float distFromBlast = Vector3.Distance(hitColliders[i].transform.position, transform.position);
			float parameterForMessage = -(explosiveDamage * blastRadius / distFromBlast);

			//Debug.Log("Dealing Damage to : " + hitColliders[i].name + "\t" + parameterForMessage + "\n");
			hitColliders[i].gameObject.SendMessage("AdjustHealth", parameterForMessage * Creator.Carrier.DamageAmplification, SendMessageOptions.DontRequireReceiver);
			i++;
		}
		Destroy(gameObject, 3.0f);
	}
}