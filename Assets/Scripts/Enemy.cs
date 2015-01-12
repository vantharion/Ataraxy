﻿using UnityEngine;
using System.Collections;

public class Enemy : Entity
{
	public Shader outline;
	public Shader diffuse;
	private bool targeted = false;
	public bool Targeted
	{
		get { return targeted; }
		set 
		{
			if (value)
			{
				counter = targetFade;
			}
			targeted = value; 
		}
	}
	private float counter = 0.0f;
	private float targetFade = 3.0f;

	void Start()
	{
		gameObject.tag = "Enemy";
		outline = Shader.Find("Outlined/Silhouetted Diffuse");
		diffuse = Shader.Find("Diffuse");
	}
	
	void Update()
	{
		if (Targeted)
		{
			counter -= Time.deltaTime;
			renderer.material.shader = outline;

			if (counter < 0)
			{
				Targeted = false;
			}
		}
		else
		{
			renderer.material.shader = diffuse;
		}

		/*if (Input.GetKey(KeyCode.M))
		{
			Debug.Log("M Held\n");
			Targeted = true;
		}
		else
		{
			Targeted = false;
		}*/
	}
}
