using UnityEngine;
using System.Collections;

public class MultiToken : MonoBehaviour 
{
	//Bunch of references to stats and such. We could trim this down honestly.
	private Player player;

	public bool createTerrain = true;
	public bool healPlayer = true;
	public bool grantWeapon = true;
	public bool grantPassive = false;
	public bool repairWeapon = false;
	public bool grantExperience = false;

	public int heal = 2;
	public int minRepair = 20;
	public int maxRepair = 45;
	public float xpReward = 10;

	//Some audio info
	public bool playOnPickup = true;
	public AudioClip acquireClip;

	// Use this for initialization
	void Start () 
	{
		player = GameManager.Instance.player;
		if (player == null)
		{
			Debug.LogError("Token cannot find player Game Object.\n");
		}
		tag = "Token";
	}

	void OnTriggerEnter(Collider collider)
	{
		//Only when we hit the player
		if (collider.gameObject.tag == "Player")
		{
			//Debug.Log("Collected");
			//Give the player a new passive/weapon.
			//Spawn new terrain
			//Spawn new enemies

			if (grantExperience)
			{
				GameManager.Instance.player.GainExperience(xpReward);
			}
			if (healPlayer)
			{
				player.AdjustHealth(heal);
			}
			GrantWeaponOrPassive();
			CreateTerrain();
			RepairWeapon();
			PlayAudio();
			DisableToken();
		}
	}

	
	#region Grant Weapons & Passives
	public void GrantWeaponOrPassive()
	{
		if (!grantPassive && !grantWeapon)
		{
			return;
		}
		if (!grantPassive && grantWeapon)
		{
			GrantWeapon();
		}
		else if (!grantWeapon && grantPassive)
		{
			GrantPassive();
		}
		else
		{
			if (player.weapons.Count == 0 || Random.Range(0, 4) != 0)
			{
				GrantWeapon();
			}
			else
			{
				GrantPassive();
			}
		}
	}

	public void GrantPassive()
	{
		player.SetupAbility(Passive.New());
	}
	public void GrantWeapon()
	{
		if (grantWeapon)
		{
			player.SetupAbility(NewWeapon());
		}
	}
	#endregion
	
	public void CreateTerrain()
	{
		if (createTerrain)
		{
			Cluster nearest = TerrainManager.Instance.FindNearestCluster(transform.position);

			if (nearest != null)
			{
				//Debug.DrawLine(transform.position, nearest.transform.position, Color.cyan, 8.0f);
			}

			TerrainManager.Instance.CreateNewCluster(nearest);
		}
	}

	public void RepairWeapon()
	{
		if (repairWeapon)
		{
			player.AdjustActiveDurability(Random.Range(minRepair, maxRepair));
		}
	}

	public void PlayAudio()
	{
		if (playOnPickup && acquireClip != null)
		{
			player.gameObject.audio.clip = acquireClip;
			player.gameObject.audio.Play();
		}
	}

	public void DisableToken()
	{
		if (light != null)
		{
			light.enabled = false;
		}
		gameObject.SetActive(false);
		renderer.enabled = false;
		if (particleSystem != null)
		{
			particleSystem.enableEmission = false;
		}
	}

	public static Ability NewWeapon()
	{
		switch(Random.Range(0, 10))
		{
			case 0:
				return RocketLauncher.New();
			case 1:
				return ShockRifle.New();
			case 2:
				return Longsword.New();
			case 3:
				return Dagger.New();
			case 4:
				return Rapier.New();
			case 5:
				return GravityStaff.New();
			case 6:
				return BoundingStaff.New();
			case 7:
				return WingedSandals.New();
			case 8:
				return GrapplingHook.New();
			//case 5:
			//	return Dagger.New();
			default:
				return Weapon.New();
		}
	}
}
