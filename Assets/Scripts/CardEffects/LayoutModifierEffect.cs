using UnityEngine;

[CreateAssetMenu(fileName = "LayoutModifierEffect", menuName = "CardEffect/LayoutModifierEffectCard", order = 1)]
public class LayoutModifierEffect : CardEffect
{
	public int SpikeModifierValue;
	public int SawModifierValue;

	public bool runNerf = false;
	public bool doubleJumpNerf = false;
	public bool gravityNerf = false;

	public bool speedBuff = false;
	public bool jumpBuff = false;

	public void ApplyEffect(MapLayout map)
	{
		map.AddSpikes(SpikeModifierValue);
		map.AddSaws(SawModifierValue);

		map.runNerf = runNerf || map.runNerf;
		map.doubleJumpNerf = doubleJumpNerf || map.doubleJumpNerf;
		map.gravityNerf = gravityNerf || map.gravityNerf;

		map.speedBuff = speedBuff || map.speedBuff;
		map.jumpBuff = jumpBuff || map.jumpBuff;
	}
}
