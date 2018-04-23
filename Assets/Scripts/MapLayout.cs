using UnityEngine;

public class MapLayout : MonoBehaviour
{
	public const int MaxSpikeModifier = 3;
	public const int MaxSawModifier = 2;

	public Transform playerSpawn;
	public Transform exit;

	public GameObject[] Spikes;
	public GameObject[] Saws;

	[HideInInspector]
	public int CurrentActiveSpikes = 0;
	[HideInInspector]
	public int CurrentActiveSaws = 0;

	[HideInInspector]
	public bool runNerf = false;
	[HideInInspector]
	public bool doubleJumpNerf = false;
	[HideInInspector]
	public bool gravityNerf = false;

	[HideInInspector]
	public bool speedBuff = false;
	[HideInInspector]
	public bool jumpBuff = false;

	void Start ()
	{
		for(int i = CurrentActiveSpikes; i < Spikes.Length; i++)
		{
			Spikes[i].SetActive(false);
		}

		for (int i = CurrentActiveSaws; i < Saws.Length; i++)
		{
			Saws[i].SetActive(false);
		}
	}

	public void AddSpikes(int spikes)
	{
		for (int i = CurrentActiveSpikes; i < CurrentActiveSpikes + spikes; i++)
		{
			Spikes[i].SetActive(true);
		}

		CurrentActiveSpikes += spikes;
	}

	public void AddSaws(int saws)
	{
		for(int i = CurrentActiveSaws; i < CurrentActiveSaws + saws; i++)
		{
			Saws[i].SetActive(true);
		}

		CurrentActiveSaws += saws;
	}

	public bool CanApplyModifier(LayoutModifierEffect effect)
	{
		if (effect.SpikeModifierValue + CurrentActiveSpikes > MaxSpikeModifier) return false;
		if (effect.SawModifierValue + CurrentActiveSaws > MaxSawModifier) return false;

		if (effect.runNerf && runNerf) return false;
		if (effect.doubleJumpNerf && doubleJumpNerf) return false;
		if (effect.gravityNerf && gravityNerf) return false;
		
		if (effect.speedBuff && speedBuff) return false;
		if (effect.jumpBuff && jumpBuff) return false;

		return true;
	}

	public void TranslateLayout(Vector3 newPosition)
	{
		Vector3 diff = newPosition - transform.position;

		transform.position = newPosition;

		Saw[] saws = GetComponentsInChildren<Saw>();

		for(int i = 0; i < saws.Length; i++)
		{
			saws[i].TranslateSaw(diff);
		}
	}
}
