using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Deck : MonoBehaviour
{
	public GameObject[] cards;

	public Transform[] slots;

	public Card cardPrefab;

	private Dictionary<Transform, Card> slotsAndCards = new Dictionary<Transform, Card>();

	private int cardDestroyedCount;
	private int totalCardsDestroyedCount;

	public CardEffect[] deck;

	private int cardCount;

	public Action DeckEnded;

	public TextMeshPro cardCountText;

	public AudioSource source;

	public void Start()
	{
		deck.Shuffle();

		cardCount = deck.Length;

		for (int i = 0; i < slots.Length && cardCount > 0; i++)
		{	
			Card card = Instantiate(cardPrefab, slots[i]);
			slotsAndCards.Add(slots[i], card);
			card.OnDestroy += CardDestroyed;
			cardCount--;
			card.cardEffect = deck[cardCount];
			card.GetComponent<SpriteRenderer>().sprite = card.cardEffect.cardImage;
		}

		CheckPile();

		cardDestroyedCount = 0;
	}

	private void CardDestroyed(Card cardDestroyed)
	{
		cardDestroyedCount++;
		totalCardsDestroyedCount++;

		if (cardDestroyedCount == slots.Length - 1 )
		{
			for (int i = 0; i < slots.Length && cardCount > 0; i++)
			{
				if(slotsAndCards[slots[i]] == null || slotsAndCards[slots[i]] == cardDestroyed)
				{
					Card card = Instantiate(cardPrefab, slots[i]);
					slotsAndCards[slots[i]] = card;
					card.OnDestroy += CardDestroyed;
					cardCount--;
					card.cardEffect = deck[cardCount];
					card.GetComponent<SpriteRenderer>().sprite = card.cardEffect.cardImage;
				}
			}

			CheckPile();

			cardDestroyedCount = 0;
		}

		if (totalCardsDestroyedCount == deck.Length)
		{
			if (DeckEnded != null)
			{
				DeckEnded.Invoke();
			}
		}
	}

	private void CheckPile()
	{
		source.Play();

		cardCountText.SetText(cardCount.ToString());

		for (int i = 0; i < cards.Length; i++)
		{
			cards[i].SetActive(cardCount / (float)deck.Length > (i + 1) / (float)cards.Length);
		}
	}
}
