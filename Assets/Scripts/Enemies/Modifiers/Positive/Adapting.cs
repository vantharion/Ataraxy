﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Adapting : Modifier
{
	public new static string[] modNames = { "Adapting" };

	public new static Adapting New()
	{
		Adapting newMod = ScriptableObject.CreateInstance<Adapting>();
		return newMod;
	}

	public override void Init()
	{
		ModifierName = modNames[Random.Range(0, modNames.Length - 1)];
		Stacks = Random.Range(1, 5);
		UIColor = new Color(Random.Range(0, .999f), Random.Range(0, .999f), Random.Range(0, .999f), .4f);
		TextColor = Color.black;
	}

	public override void Gained(int stacksGained = 0, bool newStack = false)
	{
		Carrier.xpRateOverTime += .5f * stacksGained;
		base.Gained(stacksGained, newStack);
	}

	public override void Update()
	{
		base.Update();
	}
}