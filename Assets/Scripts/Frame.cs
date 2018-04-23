using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Frame : MonoBehaviour
{
	public Action<MapLayout, Frame> OnNewMap;
	public Action<Frame> OnCardApplied;

	public MapLayout mapLayout;

	public TextMeshPro SpikesCountText;
	public TextMeshPro SawsCountText;

	public GameObject GravityNerfIcon;
	public GameObject NoDoubleJumpNerfIcon;
	public GameObject NoRunNerfIcon;

	public GameObject SpeedBuffIcon;
	public GameObject JumpBuffIcon;

	[HideInInspector]
	public List<LayoutModifierEffect> layoutModifiers = new List<LayoutModifierEffect>();

	public AudioSource source;

	public AudioClip applyBuffSound;
	public AudioClip applyNerfSound;

	public AudioClip applyGravitySound;
	public AudioClip applySawSound;
	public AudioClip applySpikeSound;

	public AudioClip newMapSound;

	public bool ApplyCard(Card card)
	{
		NewLayoutEffect effect = card.cardEffect as NewLayoutEffect;

		if (effect == null)
		{
			LayoutModifierEffect layoutEffect = card.cardEffect as LayoutModifierEffect;

			if (mapLayout != null)
			{
				if (!mapLayout.CanApplyModifier(layoutEffect))
				{
					return false;
				}

				layoutEffect.ApplyEffect(mapLayout);

				PlaySound(layoutEffect);

				if (OnCardApplied != null)
				{
					OnCardApplied.Invoke(this);
				}

				return true;
			}
			else
			{
				int spikeModifierValue = 0;
				int sawModifierValue = 0;
				foreach (LayoutModifierEffect modifier in layoutModifiers)
				{
					spikeModifierValue += modifier.SpikeModifierValue;
					sawModifierValue += modifier.SawModifierValue;
				}

				if (spikeModifierValue + layoutEffect.SpikeModifierValue > MapLayout.MaxSpikeModifier)
				{
					return false;
				}

				if (sawModifierValue + layoutEffect.SawModifierValue > MapLayout.MaxSawModifier)
				{
					return false;
				}

				layoutModifiers.Add(layoutEffect);

				PlaySound(layoutEffect);

				if (OnCardApplied != null)
				{
					OnCardApplied.Invoke(this);
				}

				return true;
			}
		}
		else
		{
			if (mapLayout == null)
			{
				if (OnNewMap != null)
				{
					OnNewMap.Invoke(effect.mapLayoutPrefab, this);
				}

				if (OnCardApplied != null)
				{
					OnCardApplied.Invoke(this);
				}

				source.PlayOneShot(newMapSound);

				return true;
			}
		}

		return false;
	}
	
	private void PlaySound(LayoutModifierEffect layoutEffect)
	{
		if (layoutEffect.jumpBuff || layoutEffect.speedBuff)
		{
			source.PlayOneShot(applyBuffSound);
		}
		else if(layoutEffect.gravityNerf)
		{
			source.PlayOneShot(applyGravitySound);
		}
		else if(layoutEffect.SawModifierValue > 0)
		{
			source.PlayOneShot(applySawSound);
		}
		else if(layoutEffect.SpikeModifierValue > 0)
		{
			source.PlayOneShot(applySpikeSound);
		}
		else
		{
			source.PlayOneShot(applyNerfSound);
		}
	}

	public void UpdateIcons()
	{
		ResetIcons();

		if (mapLayout != null)
		{
			SpikesCountText.SetText(mapLayout.CurrentActiveSpikes.ToString());
			SawsCountText.SetText(mapLayout.CurrentActiveSaws.ToString());
			GravityNerfIcon.SetActive(mapLayout.gravityNerf);
			NoDoubleJumpNerfIcon.SetActive(mapLayout.doubleJumpNerf);
			NoRunNerfIcon.SetActive(mapLayout.runNerf);
			JumpBuffIcon.SetActive(mapLayout.jumpBuff);
			SpeedBuffIcon.SetActive(mapLayout.speedBuff);
		}
		else
		{
			int spikeModifierValue = 0;
			int sawModifierValue = 0;
			foreach (LayoutModifierEffect modifier in layoutModifiers)
			{
				spikeModifierValue += modifier.SpikeModifierValue;
				sawModifierValue += modifier.SawModifierValue;

				GravityNerfIcon.SetActive(modifier.gravityNerf || GravityNerfIcon.activeInHierarchy);
				NoDoubleJumpNerfIcon.SetActive(modifier.doubleJumpNerf || NoDoubleJumpNerfIcon.activeInHierarchy);
				NoRunNerfIcon.SetActive(modifier.runNerf || NoRunNerfIcon.activeInHierarchy);
				JumpBuffIcon.SetActive(modifier.jumpBuff || JumpBuffIcon.activeInHierarchy);
				SpeedBuffIcon.SetActive(modifier.speedBuff || SpeedBuffIcon.activeInHierarchy);
			}

			SpikesCountText.SetText(spikeModifierValue.ToString());
			SawsCountText.SetText(sawModifierValue.ToString());
		}
	}

	public void ResetIcons()
	{
		SpikesCountText.SetText("0");
		SawsCountText.SetText("0");
		GravityNerfIcon.SetActive(false);
		NoDoubleJumpNerfIcon.SetActive(false);
		NoRunNerfIcon.SetActive(false);
		JumpBuffIcon.SetActive(false);
		SpeedBuffIcon.SetActive(false);
	}

	public bool CanApplyCard(Card card)
	{
		NewLayoutEffect effect = card.cardEffect as NewLayoutEffect;

		if (effect == null)
		{
			LayoutModifierEffect layoutEffect = card.cardEffect as LayoutModifierEffect;

			if (mapLayout != null)
			{
				return mapLayout.CanApplyModifier(layoutEffect);
			}
			else
			{
				int spikeModifierValue = 0;
				int sawModifierValue = 0;
				foreach (LayoutModifierEffect modifier in layoutModifiers)
				{
					spikeModifierValue += modifier.SpikeModifierValue;
					sawModifierValue += modifier.SawModifierValue;
				}

				if (spikeModifierValue + layoutEffect.SpikeModifierValue > MapLayout.MaxSpikeModifier)
				{
					return false;
				}

				if (sawModifierValue + layoutEffect.SawModifierValue > MapLayout.MaxSawModifier)
				{
					return false;
				}

				return true;
			}
		}
		else
		{
			if (mapLayout == null) return true;
		}
		
		return false;
	}
}
