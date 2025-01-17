﻿using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Player : Entity
{
	#region Lists - Weapons & Passives
	public List<Weapon> weapons;
	public List<Passive> passives;
	#endregion

	#region Player Unique Interface
	public Image SelectorUI;
	public Image CrosshairUI;
	public Crosshair playerCrosshair;
	public Text WeaponText;
	public GameObject WeaponUI;
	public GameObject PassiveUI;
	public GameObject iconPrefab;

	public float flashSpeed = 5f;
	public Color flashColor = new Color(1f, 0f, 0f, 0.15f);
	#endregion

	#region Key GameObjects - Camera, Target
	public Camera mainCamera;
	public NPC targetedEntity = null;
	public GameObject hitscanTarget = null;
	public Vector3 targetScanDir = Vector3.zero;
	public Vector3 hitscanContact = Vector3.zero;
	public GameObject leftShFirePoint;
	public GameObject rightShFirePoint;
	public GameObject leftHipFirePoint;
	public GameObject rightHipFirePoint;

	public TeleTarget teleTarget;
	#endregion

	#region Weapon Variables
	private float targetFadeCounter = 0.0f;

	private int weaponIndex = 0;
	public int WeaponIndex
	{
		get { return weaponIndex; }
		set { weaponIndex = value; }
	}
	#endregion

	#region Player Stats
	//Damage Amplification
	//Invincibility Frames
	//Double Jump
	//Experience per Level
	//Damage Reduction
	//Bonus Knockback
	//Critical Hit Chance
	
	#endregion

	#region XP & Resource System
	public enum ResourceSystem { Mana, Rage, Energy };
	public ResourceSystem rSystem = ResourceSystem.Energy;

	public override void GainLevel()
	{

		AudioManager.Instance.MakeSource("LevelUp").Play();
		MaxHealth += 5;
		AdjustHealth(5);
		
		base.GainLevel();

		SetupHealthUI();
		SetupXPUI();
		XPNeeded += 50;
	}

	void SetupResourceSystem()
	{

	}

	public override void SetupNameUI()
	{
		
	}

	void ManageResourceSystem()
	{
		switch (rSystem)
		{
			case ResourceSystem.Mana:

				break;
			case ResourceSystem.Rage:

				break;
			case ResourceSystem.Energy:
				//Check distance from nearby enemies.

				break;
		}

	}
	#endregion

	#region Core Functions - Start, Update, GetInput
	#region Initialization
	public override void Start()
	{
		NameInGame = "Vant";

		#region Firing Points
		//Collect our firing points. These are used by weapons to pick start points for projectiles.
		List<GameObject> fPoints = new List<GameObject>();
		fPoints.Add(transform.FindChild("Main Camera").transform.FindChild("Front Firing Point").gameObject);
		fPoints.Add(transform.FindChild("Main Camera").transform.FindChild("RightShoulder Firing Point").gameObject);
		fPoints.Add(transform.FindChild("Main Camera").transform.FindChild("LeftShoulder Firing Point").gameObject);
		fPoints.Add(transform.FindChild("Main Camera").transform.FindChild("RightHip Firing Point").gameObject);
		fPoints.Add(transform.FindChild("Main Camera").transform.FindChild("LeftHip Firing Point").gameObject);

		FirePoints = fPoints.ToArray();
		#endregion

		#region Gather UI Elements
		//Assign UI elements.
		HealthSlider = UIManager.Instance.player_HP;
		HealthText = UIManager.Instance.player_HPText;
		XPSlider = UIManager.Instance.player_XP;
		XPText = UIManager.Instance.player_XPText;
		LevelText = UIManager.Instance.player_LevelText;
		NameText = UIManager.Instance.player_Name;
		ResourceSlider = UIManager.Instance.player_Resource;
		ResourceText = UIManager.Instance.player_ResourceText;
		WeaponText = UIManager.Instance.player_WeaponText;
		SelectorUI = UIManager.Instance.player_Selector;
		CrosshairUI = UIManager.Instance.player_Crosshair;
		playerCrosshair = CrosshairUI.GetComponent<Crosshair>();
		WeaponUI = UIManager.Instance.player_WeaponFolder;
		PassiveUI = UIManager.Instance.player_PassiveFolder;

		//Get UI elements
		SelectorUI.gameObject.SetActive(true);
		SelectorUI.fillMethod = Image.FillMethod.Radial360;
		SelectorUI.fillClockwise = true;
		#endregion

		//Keeping a list of weapons and passives that affect ourself.
		weapons = new List<Weapon>();
		passives = new List<Passive>();

		//Give the player starting stuff
		GiveStartingItems();

		//Stub method currently. For if we add mana/other things
		SetupResourceSystem();

		Level = 1;
		XPNeeded = 30;

		//Set the values for all the different UI elements.
		SetupHealthUI();
		SetupResourceUI();
		//SetupNameUI();
		SetupXPUI();

		DamageImage = UIManager.Instance.damage_Indicator;

		if (teleTarget == null)
		{
			teleTarget = GetComponent<TeleTarget>();
		}

		base.Start();

		gameObject.tag = "Player";
		Faction = Allegiance.Player;
	}

	public void GiveStartingItems()
	{

        if (Constants.GameDifficulty == 0)
        {

        }
        else
        {

#if UNITY_EDITOR
            SetupAbility(LootManager.NewWeapon("GravityStaff"));
            SetupAbility(LootManager.NewWeapon("GrapplingHook"));
            /*
            SetupAbility(LootManager.NewWeapon("BoundingStaff"));
            SetupAbility(LootManager.NewWeapon("WingedSandals"));
            SetupAbility(LootManager.NewWeapon("Longsword"));
            SetupAbility(LootManager.NewWeapon("Rapier"));
            SetupAbility(LootManager.NewWeapon("Dagger"));
            SetupAbility(LootManager.NewWeapon("RocketLauncher"));
            SetupAbility(LootManager.NewWeapon("Hemotick"));
            SetupAbility(LootManager.NewWeapon("ShockRifle"));*/
#else
		SetupAbility(LootManager.NewWeapon("GravityStaff"));
		SetupAbility(LootManager.NewWeapon("GrapplingHook"));
#endif
        }
	}

	public void SetupAbility(Ability ToAdd)
	{
		if (ToAdd is Weapon)
		{
			if (weapons.Count == 0)
			{
				weaponIndex = 0;
			}

			Weapon w = (Weapon)ToAdd;

			w.Init();

			//WeaponUI
			Image panel = ((GameObject)GameObject.Instantiate(iconPrefab)).GetComponent<Image>();

			panel.name = "I: " + w.AbilityName;

			w.Faction = Allegiance.Player;

			panel.rectTransform.SetParent(WeaponUI.transform);
			w.Remainder = panel.transform.FindChild("Remainder").GetComponent<Text>();
			
			w.Remainder.text = w.Durability.ToString();
			w.Carrier = this;
			
			panel.sprite = w.Icon;
			w.IconUI = panel;
			weapons.Add(w);
			panel.rectTransform.anchoredPosition = new Vector2((weapons.Count - 1) * 67, 0);

		}
		else if (ToAdd is Passive)
		{
			Passive p = (Passive)ToAdd;

			Image panel = ((GameObject)GameObject.Instantiate(iconPrefab)).GetComponent<Image>();

			panel.name = "I: " + p.AbilityName;

			panel.rectTransform.anchorMin = new Vector2(1, 1);
			panel.rectTransform.anchorMax = new Vector2(1, 1);
			
			p.Icon = UIManager.Instance.Icons[Random.Range(1, UIManager.Instance.Icons.Length)];
			panel.color = new Color(0, .8f, 0);
			panel.rectTransform.SetParent(PassiveUI.transform);
			p.Remainder = panel.transform.FindChild("Remainder").GetComponent<Text>();
			p.Remainder.text = ((int)(p.DurationRemaining * 10)).ToString();

			panel.sprite = p.Icon;
			p.IconUI = panel;
			passives.Add(p);

			panel.rectTransform.anchoredPosition = new Vector2((passives.Count) * -67, 0);
		}
	}
	#endregion

	#region Update, MaintainAbilities
	public override void Update()
	{
		//Vector3 wFor = new Vector3((transform.position + transform.forward * 10).x, transform.position.y, (transform.position + transform.forward * 10).z);
		//Vector3 wCamFor = new Vector3((transform.position + GetForward() * 10).x, transform.position.y, (transform.position + GetForward() * 10).z);
		//Vector3 wVel = new Vector3((transform.position + rigidbody.velocity * 10).x, transform.position.y, (transform.position + rigidbody.velocity * 10).z);
		//Vector3 wScan = new Vector3(targetScanDir.x, transform.position.y, targetScanDir.z);

		//Debug.DrawLine(transform.position, wFor, Color.blue, .02f);
		//Debug.DrawLine(transform.position, wCamFor, Color.yellow, .02f);
		//Debug.DrawLine(transform.position, wVel, Color.cyan, .02f);
		//Debug.DrawLine(transform.position, wScan, Color.magenta, .02f);

		GetInput();
		if (!UIManager.Instance.paused)
		{
			//Debug.Log("Target: " + targetedEntity + "\n");
			//Debug.DrawLine(transform.position,TerrainManager.Instance.clusters[ TerrainManager.Instance.FindNearestCluster(transform.position)].transform.position, Color.red);

			/*if ((null != TerrainManager.Instance.FindNearestCluster(transform.position, 15)))
			{
				Debug.DrawLine(transform.position, TerrainManager.Instance.FindNearestCluster(transform.position, 15).transform.position, Color.white);
			}*/
			//Debug.DrawLine(transform.position, transform.position + TerrainManager.Instance.FindOffsetOfDir(tempIndex % 8), Color.cyan, 3f);

			#region Handle Damage
			if (Damaged)
			{
				//AudioManager.Instance.MakeSource("Hurt").Play();
				DamageImage.color = flashColor;
			}
			else
			{
				if (DamageImage != null)
				{
					DamageImage.color = Color.Lerp(DamageImage.color, Color.clear, flashSpeed * Time.deltaTime);
				}
			}
			#endregion
			#region Resource System
			ManageResourceSystem();
			#endregion
			bool dirtyAbilityBar = false;
			#region Update Passive Durations & Check Removal
			for (int i = 0; i < passives.Count; i++)
			{
				if (passives[i] != null)
				{
					passives[i].UpdatePassive(Time.deltaTime);
					if (passives[i].CheckAbility())
					{
						passives.RemoveAt(i);
						i--;
						dirtyAbilityBar = true;
					}
				}
			}
			#endregion
			#region Check Weapons for Removal

            if (weapons.Count == 0)
            {
                WeaponText.text = "You are unarmed";
				weaponIndex = 0;
            }
            else
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] != null)
                    {
                        if (weaponIndex == i)
                        {
                            WeaponText.text = weapons[i].AbilityName;
                            weapons[i].UpdateCrosshair(playerCrosshair, hitscanContact);

                            if (weapons[i].CdLeft > 0)
                            {
                                SelectorUI.type = Image.Type.Filled;
                                SelectorUI.fillCenter = true;
                                float startCooldownAmt = 0;
                                if (weapons[i].UseSpecialCooldown)
                                {
                                    startCooldownAmt = weapons[i].SpecialCooldown;
                                }
                                else
                                {
                                    startCooldownAmt = weapons[i].NormalCooldown;
                                }
                                SelectorUI.fillAmount = 1 - (weapons[i].CdLeft / startCooldownAmt);
                            }
                            else
                            {
                                //SelectorUI.fillAmount = 1;
                                SelectorUI.type = Image.Type.Sliced;
                                SelectorUI.fillCenter = false;
                            }
                        }
                        weapons[i].UpdateWeapon(Time.deltaTime);
                        if (weapons[i].CheckAbility())
                        {

                            weapons[i].CleanUp();
                            weapons.RemoveAt(i);
                            i--;
                            dirtyAbilityBar = true;
                        }
                    }
                }
            }

			if (weaponIndex > weapons.Count - 1)
			{
				weaponIndex = weapons.Count - 1;
			}

			if (dirtyAbilityBar)
			{
				MaintainAbilities();
			}
			#endregion
			#region Handle Selector Location
			//Set WeaponUI width equal to n * 67
			// 1 = 64
			// 64 * n + ((n-1)*3)
			//WeaponUI.rectTransform.rect = new Rect(
            
            float halfScreen = Screen.width / 2;
            float wC = (weapons.Count - 1) * 67 / 2;
            float wI = (WeaponIndex) * 67;
            if (weapons.Count > 0)
            {
                SelectorUI.rectTransform.position = new Vector3(halfScreen - wC + wI, 35 + 20);
            }
            else
            {
                SelectorUI.rectTransform.position = new Vector3(halfScreen - wC + wI, -35 - 20);
            }
            int index = SelectorUI.transform.GetSiblingIndex();
            SelectorUI.transform.SetSiblingIndex(index + 1);
         
			#endregion

			//Try to find if our cursor is targetting something
			hitscanTarget = TargetScan();
			/*if (hitscanTarget != null)
			{
				Debug.Log(hitscanTarget.name);
			}*/
			UIManager.Instance.player_WeaponFolder.GetComponent<RectTransform>().anchoredPosition = new Vector2((Screen.width / 2) - ((weapons.Count) * 67) / 2, 0);
		
			HandleLoseTarget();

			Damaged = false;

			base.Update();
		}
		else
		{
			//This would ideally only be set when the player pauses. Not an important optimization.
			if (weapons.Count > 0 && weaponIndex >= 0)
			{
				UIManager.Instance.item_NameText.text = weapons[weaponIndex].AbilityName;
				UIManager.Instance.item_PrimaryText.text = weapons[weaponIndex].PrimaryDesc;
				UIManager.Instance.item_SecondaryText.text = weapons[weaponIndex].SecondaryDesc;
			}
		}
	}

	public void MaintainAbilities()
	{
		for (int i = 0; i < weapons.Count; i++)
		{
			//weapons[i].IconUI.rectTransform.anchoredPosition = new Vector2((Screen.width - (i) * 67)/ 2 , 0);
			weapons[i].IconUI.rectTransform.anchoredPosition = new Vector2((i) * 67, 0);
		}
		for (int i = 0; i < passives.Count; i++)
		{
			passives[i].IconUI.rectTransform.anchorMin = new Vector2(1, 1);
			passives[i].IconUI.rectTransform.anchorMax = new Vector2(1, 1);

			passives[i].IconUI.rectTransform.anchoredPosition = new Vector2((i + 1) * -67, 0);
		}
	}
	#endregion

	void GetInput()
	{
		if (!UIManager.Instance.paused)
		{
			#region Mouse Buttons 1
			if (Input.GetButtonDown("Fire1"))
			{
				//Debug.Log("Firing\n");
			}
			if (Input.GetButton("Fire1"))
			{
				//If we have a weapon
				if (weapons.Count > 0)
				{
					//If the weapon has ammo and is off cooldown, DO IT.
					if (weapons[weaponIndex].HandleDurability(false))
					{
						//If we have the same target this frame as our HUD
						if (targetedEntity != null && hitscanTarget != null && hitscanTarget == targetedEntity.gameObject)
						{
							weapons[weaponIndex].UseWeapon(targetedEntity.gameObject, targetedEntity.GetType(), FirePoints, targetScanDir, true);
						}
						else
						{
							weapons[weaponIndex].UseWeapon(hitscanTarget, null, FirePoints, targetScanDir, true);
						}
					}

				}
			}
			if (Input.GetButtonUp("Fire1"))
			{
				//Debug.Log("Fire Ceased\n");
			}
			#endregion

			#region Mouse Buttons 2
			if (Input.GetButtonDown("Fire2"))
			{
				//Debug.Log("Firing\n");
			}
			if (Input.GetButton("Fire2"))
			{
				//If we have a weapon
				if (weapons.Count > 0)
				{
					//If the weapon has ammo and is off cooldown, DO IT.
					if (weapons[weaponIndex].HandleDurability(true))
					{
						//If we have the same target this frame as our HUD
						if (targetedEntity != null && hitscanTarget != null && hitscanTarget == targetedEntity.gameObject)
						{
							weapons[weaponIndex].UseWeaponSpecial(targetedEntity.gameObject, targetedEntity.GetType(), FirePoints, targetScanDir, true);
						}
						else
						{
							weapons[weaponIndex].UseWeaponSpecial(hitscanTarget, null, FirePoints, targetScanDir, true);
						}
					}

				}
			}
			if (Input.GetButtonUp("Fire2"))
			{
				//Debug.Log("Fire Ceased\n");
			}
			#endregion
		
			#region Scroll Wheel
			if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetButtonDown("Previous Weapon"))
			{
				if (weaponIndex == 0)
				{
					weaponIndex = weapons.Count - 1;
				}
				else
				{
					weaponIndex -= 1;
				}
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetButtonDown("Next Weapon"))
			{
				if (weaponIndex == weapons.Count - 1)
				{
					weaponIndex = 0;
				}
				else
				{
					weaponIndex += 1;
				}
			}
			#endregion

			#region Number Checking
			if (Input.GetButton("Quickslot 1"))
			{
				weaponIndex = 0;
			}
			if (Input.GetButton("Quickslot 2"))
			{
				weaponIndex = 1;
			}
			if (Input.GetButton("Quickslot 3"))
			{
				weaponIndex = 2;
			}
			if (Input.GetButton("Quickslot 4"))
			{
				weaponIndex = 3;
			}
			if (Input.GetButton("Quickslot 5"))
			{
				weaponIndex = 4;
			}
			if (Input.GetButton("Quickslot 6"))
			{
				weaponIndex = 5;
			}
			if (Input.GetButton("Quickslot 7"))
			{
				weaponIndex = 6;
			}
			if (Input.GetButton("Quickslot 8"))
			{
				weaponIndex = 7;
			}
			if (Input.GetButton("Quickslot 9"))
			{
				weaponIndex = 8;
			}
			if (Input.GetButton("Quickslot 10"))
			{
				weaponIndex = 9;
			}
			#endregion

			#region Unity Editor Only
			#if UNITY_EDITOR || CHEAT

			if (Input.GetKeyDown(KeyCode.Semicolon))
			{
				AdjustActiveDurability(15);
			}
			if (Input.GetKeyDown(KeyCode.Quote))
			{
				AdjustActiveDurability(-15);
			}

			#region Dev Movement Buttons
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				Application.LoadLevel(Application.loadedLevel);
			}
			
			//Go up
			if (Input.GetKey(KeyCode.T))
			{
				Vector3 newVel = new Vector3(0.0f, 1, 0.0f);
				newVel.Normalize();
				rigidbody.velocity = Vector3.zero;
				ExternalMove(newVel, 30, ForceMode.VelocityChange);
			}
			//Go down
			if (Input.GetKey(KeyCode.G))
			{
				Vector3 newVel = new Vector3(0.0f, -1, 0.0f);
				newVel.Normalize();
				rigidbody.velocity = Vector3.zero;
				ExternalMove(newVel, 40, ForceMode.VelocityChange);
			}
			//Go Forward
			if (Input.GetKey(KeyCode.LeftShift))
			{
				Vector3 newVel = new Vector3(GetForward().x, 0, GetForward().z);
				newVel.Normalize();
				rigidbody.velocity = Vector3.zero;
				ExternalMove(newVel, 110, ForceMode.VelocityChange);
			}
			//Stops all player movement.
			if (Input.GetKeyDown(KeyCode.LeftControl))
			{
				rigidbody.velocity = Vector3.zero;
			}
			//Toggles gravity. 
			if (Input.GetKeyDown(KeyCode.LeftControl))
			{
				rigidbody.useGravity = !rigidbody.useGravity;
			}
			#endregion

			#region Health & Resources
			if (Input.GetKeyDown(KeyCode.R))
			{
				AdjustHealth(-1);
			}
			#endregion

			#region Cheat Weapons
			if (Input.GetKeyDown(KeyCode.M))
			{
				SetupAbility(LootManager.NewWeapon("Longsword"));
				SetupAbility(LootManager.NewWeapon("Rapier"));
				SetupAbility(LootManager.NewWeapon("Dagger"));
				SetupAbility(LootManager.NewWeapon("RocketLauncher"));
				SetupAbility(LootManager.NewWeapon("ShockRifle"));
			}
			if (Input.GetKeyDown(KeyCode.N))
			{

				SetupAbility(LootManager.NewWeapon("Hemotick"));
				SetupAbility(LootManager.NewWeapon("GlacialSling"));
				SetupAbility(LootManager.NewWeapon("TransmuterKit"));
				SetupAbility(LootManager.NewWeapon("WingedSandals"));

			}
			if (Input.GetKeyDown(KeyCode.H))
			{
				SetupAbility(LootManager.NewWeapon("GravityStaff"));
				SetupAbility(LootManager.NewWeapon("BoundingStaff"));
				SetupAbility(LootManager.NewWeapon("WarpStaff"));
				SetupAbility(LootManager.NewWeapon("GrapplingHook"));
				SetupAbility(LootManager.NewWeapon("ShockRifle"));
			}
			#endregion
			#endif
			#endregion

			#region Sensitivity Controls
			if (Input.GetKeyDown(KeyCode.LeftBracket))
			{
				MouseView look = gameObject.GetComponent<MouseView>();

				look.sensitivity.x--;
				//look.sensitivity.y--;

				MouseView yLook = GameObject.Find("Main Camera").GetComponent<MouseView>();

				//yLook.sensitivity.x--;
				yLook.sensitivity.y--;
			}
			if (Input.GetKeyDown(KeyCode.RightBracket))
			{
				MouseView look = gameObject.GetComponent<MouseView>();

				look.sensitivity.x++;
				//look.sensitivity.y++;

				MouseView yLook = GameObject.Find("Main Camera").GetComponent<MouseView>();

				//yLook.sensitivity.x++;
				yLook.sensitivity.y++;
			}
			#endregion
		}
	}
	#endregion
	
	public override Vector3 GetForward()
	{
		return mainCamera.transform.forward;
	}

	public void ResetToCheckPoint()
	{
		//Put the player there.
		transform.position = teleTarget.lastCheckpoint.transform.position;
		transform.rotation = teleTarget.lastCheckpoint.transform.rotation;

		//Stop their movement
		rigidbody.velocity = Vector3.zero;
	}

	public override void KillEntity()
	{
		if (Constants.GameDifficulty == 0)
		{
			AdjustHealth(MaxHealth / 2);
			ResetToCheckPoint();
			FellBelow();
		}
		else
		{
			base.KillEntity();

			Application.LoadLevel("GameOver");
		}
	}

	public void FellBelow()
	{
		for (int i = 0; i < weapons.Count; i++)
		{
			if (weapons[i] is GrapplingHook)
			{
				((GrapplingHook)weapons[i]).RemoveProjectile(0);
				//Debug.Log(i + " is a grappling hook\n");
			}
		}
	}

	public void AdjustActiveDurability(int amount)
	{
		if (weapons.Count > 0)
		{
			weapons[weaponIndex].Durability += amount;
			weapons[weaponIndex].HandleVisuals();
		}
	}

	#region Targetting and Hitscan
	GameObject TargetScan()
	{
		//Where the mouse is currently targeting.
		Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hit;
		//Debug.DrawLine(transform.position, (transform.position + ray) * 100, Color.green);

		//If we fire, set targetScanDir to someplace arbitrarily far away in the shooting. Even if we hit something, we want to target wherever the cursor pointed.
		targetScanDir = transform.position + (ray.direction * 500);

		//Mask so we don't consider targeting ourself.
		LayerMask layerMask = ~((1 << 2) | (1 << 8));
		//LayerMask layerMask = ~(1 << LayerMask.NameToLayer ("Player"));

		//If we hit something
		if (Physics.Raycast(ray, out hit, 1500, layerMask))
		{
			hitscanContact = hit.point;
			//Handle cases for what we hit.

			//Debug.Log(hit.collider.gameObject.tag + "\n");
			if (hit.collider.gameObject.tag == "NPC")
			{
				NPC n = hit.collider.gameObject.GetComponent<NPC>();
				CheckNewTarget((NPC)n);

				return n.gameObject;
			}
			else if (hit.collider.gameObject.tag == "Enemy")
			{
				Enemy e = hit.collider.gameObject.GetComponent<Enemy>();
				CheckNewTarget((Enemy)e);

				return e.gameObject;
			}
			//This is outdated. You used to be able to target terrain.
			else if (hit.collider.gameObject.tag == "WorldObject")
			{
				//Island e = hit.collider.gameObject.GetComponent<Island>();
				//CheckNewTarget((Island)e);
			}
			else if (hit.collider.gameObject.tag == "Projectile")
			{
				Projectile p = hit.collider.gameObject.GetComponent<Projectile>();
				if (p != null)
				{
					return p.gameObject;
				}
			}
			else
			{
				//Catch all case.
				return hit.collider.gameObject;
			}
			//Debug.Log(hit.collider.gameObject.name + "\n");
		}
		else
		{
			hitscanContact = targetScanDir;
			//We didn't hit anything. We're about to return null as the default.
			//Debug.Log("Targetting nothing\n");
		}
		return null;
	}

	void CheckNewTarget(NPC newTarget)
	{
		//If we had a target
		if (targetedEntity != null)
		{
			//If our new target is different
			//if (newTarget != targetedEntity)
			//{
				//Untarget the old.
				targetedEntity.Untarget();

				//Set new target.
				targetedEntity = newTarget;

				//Tell em they're fabulous
				targetedEntity.Target();
			//}
		}
		//If we had no target
		else
		{
			//Set new target.
			targetedEntity = newTarget;

			//Tell em they're fabulous
			targetedEntity.Target();
		}
		targetFadeCounter = Constants.targetFade;
	}

	/// <summary>
	/// Drops target if we haven't looked at them for several seconds (whatever counter is set to)
	/// </summary>
	void HandleLoseTarget()
	{
		if (targetedEntity != null)
		{
			targetFadeCounter -= Time.deltaTime;
			if (targetFadeCounter <= 0)
			{
				//Debug.LogWarning("Dropping Target: Counter (" + targetFadeCounter + ")\n");
				targetedEntity.Untarget();
				targetedEntity = null;
			}
		}
	}
	#endregion

	#region Unneeded Code
	public void MoveInHierarchy(int delta)
	{
		int index = transform.GetSiblingIndex();
		transform.SetSiblingIndex(index + delta);
	}
	#endregion
}
