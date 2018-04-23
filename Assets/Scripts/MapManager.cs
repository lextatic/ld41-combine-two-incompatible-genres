using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
	public Transform[] mapPositions;

	public Frame[] frames;

	public MapLayout initialMapLayout;

	private Dictionary<Transform, MapLayout> slotsAndMapLayouts = new Dictionary<Transform, MapLayout>();

	public CharacterMotor Player;

	public Deck deck;

	private bool blockShift = false;
	private int layoutCardsCount;
	private int shiftsCount = 0;

	public GameObject exitPrefab;

	public GameObject victoryPanel;

	private bool deckEnded = false;

	void Start ()
	{
		for(int i = 0; i < deck.deck.Length; i++)
		{
			if(deck.deck[i] is NewLayoutEffect)
			{
				layoutCardsCount++;
			}
		}

		for (int i = 0; i < mapPositions.Length; i++)
		{
			slotsAndMapLayouts.Add(mapPositions[i], null);
			frames[i].OnNewMap += NewMap;
			frames[i].OnCardApplied += CardApplied;
			frames[i].UpdateIcons();
		}

		MapLayout initialMap = Instantiate(initialMapLayout, mapPositions[0]);
		slotsAndMapLayouts[mapPositions[0]] = initialMap;
		frames[0].mapLayout = initialMap;

		Player.ResetCharacterMotion();
		Player.transform.position = initialMap.playerSpawn.position;
		Player.OnEnterDeathTrap += PlayerDeath;
		Player.OnLeftLayout += PlayerLeftLayout;
		Player.OnFinishGame += PlayerFinishedGame;

		deck.DeckEnded += DeckEnded;
	}
	
	private void PlayerDeath()
	{
		Player.ResetCharacterMotion();

		for(int i = 0; i < mapPositions.Length; i++)
		{
			if (slotsAndMapLayouts[mapPositions[i]] != null)
			{
				Player.transform.position = slotsAndMapLayouts[mapPositions[i]].playerSpawn.position;
				ApplyModifiersOnPlayer(slotsAndMapLayouts[mapPositions[i]]);
				break;
			}
		}
	}

	private void PlayerLeftLayout(MapLayout mapLayout)
	{
		if(slotsAndMapLayouts[mapPositions[0]] == mapLayout && slotsAndMapLayouts[mapPositions[1]] != null)
		{
			ShiftLayouts();
		}
	}

	private int playerFrame = 0;

	private void Update()
	{
		if(Player.transform.position.x > 13.5f && !blockShift && slotsAndMapLayouts[mapPositions[2]] != null)
		{
			ShiftLayouts();
			shiftsCount--;
			ShiftLayouts();
		}

		if(Player.transform.position.x < -13)
		{
			if (playerFrame != 0)
			{
				playerFrame = 0;

				if (slotsAndMapLayouts[mapPositions[0]] != null)
				{
					ApplyModifiersOnPlayer(slotsAndMapLayouts[mapPositions[0]]);
				}
				else
				{
					ApplyFrameModifiersOnPlayer(frames[0]);
				}
			}
		}
		else if (Player.transform.position.x > 13)
		{
			if (playerFrame != 2)
			{
				playerFrame = 2;

				if (slotsAndMapLayouts[mapPositions[2]] != null)
				{
					ApplyModifiersOnPlayer(slotsAndMapLayouts[mapPositions[2]]);
				}
				else
				{
					ApplyFrameModifiersOnPlayer(frames[2]);
				}
			}
		}
		else
		{
			if (playerFrame != 1)
			{
				playerFrame = 1;

				if (slotsAndMapLayouts[mapPositions[1]] != null)
				{
					ApplyModifiersOnPlayer(slotsAndMapLayouts[mapPositions[1]]);
				}
				else
				{
					ApplyFrameModifiersOnPlayer(frames[1]);
				}
			}
		}
	}

	private void ApplyModifiersOnPlayer(MapLayout layout)
	{
		Player.SetCanRun(!layout.runNerf);
		Player.canDoubleJump = !layout.doubleJumpNerf;
		if(layout.gravityNerf)
		{
			Player._jumpHeight = 3.25f;
			Player._timeToPeak = 0.3f;
		}
		else
		{
			Player._jumpHeight = 4.25f;
			Player._timeToPeak = 0.35f;
		}

		if(layout.speedBuff)
		{
			Player.maxHorizontalSpeed = 10;
		}
		else
		{
			Player.maxHorizontalSpeed = 7;
		}

		if (layout.jumpBuff)
		{
			Player._jumpHeight += 1f;
		}


		Player.RecalculateGravity();
	}

	private void ApplyFrameModifiersOnPlayer(Frame frame)
	{
		Player.SetCanRun(true);
		Player.canDoubleJump = true;
		Player._jumpHeight = 4.25f;
		Player._timeToPeak = 0.35f;
		Player.maxHorizontalSpeed = 7;

		for (int i = 0; i < frame.layoutModifiers.Count; i++)
		{
			ApplyModifiersOnPlayer(frame.layoutModifiers[i]);
		}

		Player.RecalculateGravity();
	}

	private void ApplyModifiersOnPlayer(LayoutModifierEffect modiferEffect)
	{
		if (modiferEffect.runNerf)
		{
			Player.SetCanRun(false);
		}

		if (modiferEffect.doubleJumpNerf)
		{
			Player.canDoubleJump = false;
		}
		 
		if (modiferEffect.gravityNerf)
		{
			Player._jumpHeight = 3.25f;
			Player._timeToPeak = 0.3f;
		}
		
		if (modiferEffect.speedBuff)
		{
			Player.maxHorizontalSpeed = 10;
		}
		
		if (modiferEffect.jumpBuff)
		{
			Player._jumpHeight += 1f;
		}
	}

	private void PlayerFinishedGame()
	{
		victoryPanel.SetActive(true);
	}

	private void ShiftLayouts()
	{
		if (blockShift) return;

		shiftsCount++;
		if (shiftsCount == layoutCardsCount - 2)
		{
			blockShift = true;
		}

		if (slotsAndMapLayouts[mapPositions[0]] != null)
		{
			Destroy(slotsAndMapLayouts[mapPositions[0]].gameObject);
		}

		for (int i = 0; i < mapPositions.Length - 1; i++)
		{
			slotsAndMapLayouts[mapPositions[i]] = slotsAndMapLayouts[mapPositions[i + 1]];
			if (slotsAndMapLayouts[mapPositions[i]] != null)
			{
				slotsAndMapLayouts[mapPositions[i]].TranslateLayout(mapPositions[i].position);
			}
			frames[i].layoutModifiers.Clear();
			frames[i].layoutModifiers.AddRange(frames[i + 1].layoutModifiers);
			frames[i].mapLayout = frames[i + 1].mapLayout;
			frames[i].UpdateIcons();
		}

		slotsAndMapLayouts[mapPositions[mapPositions.Length - 1]] = null;
		frames[mapPositions.Length - 1].layoutModifiers.Clear();
		frames[mapPositions.Length - 1].mapLayout = null;
		frames[mapPositions.Length - 1].UpdateIcons();

		Player.transform.Translate(-26, 0, 0);
	}

	private void NewMap(MapLayout layoutPrefab, Frame frame)
	{
		int frameIndex = 0;
		for(int i = 0; i < frames.Length; i++)
		{
			if(frames[i] == frame)
			{
				frameIndex = i;
				break;
			}
		}

		MapLayout newMap = Instantiate(layoutPrefab, mapPositions[frameIndex]);
		slotsAndMapLayouts[mapPositions[frameIndex]] = newMap;
		frame.mapLayout = newMap;

		foreach(LayoutModifierEffect effect in frame.layoutModifiers)
		{
			effect.ApplyEffect(newMap);
		}

		frame.layoutModifiers.Clear();
		frame.UpdateIcons();

		if (blockShift && frameIndex == frames.Length - 1 && deckEnded)
		{
			Instantiate(exitPrefab, newMap.exit);
		}
	}

	private void CardApplied(Frame frame)
	{
		int frameIndex = 0;
		for (int i = 0; i < frames.Length; i++)
		{
			if (frames[i] == frame)
			{
				frameIndex = i;
				break;
			}
		}

		if (Player.transform.position.x > - 26 - 13 + (frameIndex * 26))
		{
			PlayerDeath();
		}

		frame.UpdateIcons();
	}

	private void DeckEnded()
	{
		deckEnded = true;

		if(blockShift && slotsAndMapLayouts[mapPositions[mapPositions.Length - 1]] != null)
		{
			Instantiate(exitPrefab, slotsAndMapLayouts[mapPositions[mapPositions.Length - 1]].exit);
		}
	}
}
