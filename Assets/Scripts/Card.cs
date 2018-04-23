using System;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
	private Vector3 offset;

	private Vector3 originalPosition;

	private List<Frame> touches = new List<Frame>();
	private Frame target;

	public Action<Card> OnDestroy;

	public CardEffect cardEffect;

	public SpriteRenderer spriteRenderer;

	public AudioSource source;
	public AudioClip getCard;
	public AudioClip releaseCard;

	void OnMouseDown()
	{
		originalPosition = transform.position;

		offset = gameObject.transform.position -
			Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));

		spriteRenderer.sortingOrder++;
		source.PlayOneShot(getCard);
	}

	void OnMouseDrag()
	{
		Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
		transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
	}

	private void OnMouseUp()
	{
		if(touches.Count == 0)
		{
			transform.position = originalPosition;
			spriteRenderer.sortingOrder--;
			source.PlayOneShot(releaseCard);
		}
		else
		{
			if(target.ApplyCard(this))
			{
				Destroy(gameObject);

				if (OnDestroy != null)
				{
					OnDestroy.Invoke(this);
				}
			}
			else
			{
				transform.position = originalPosition;
				spriteRenderer.sortingOrder--;
				source.PlayOneShot(releaseCard);
			}
		}
	}

	public Color OriginalFrameColor;

	private void Update()
	{
		if(touches.Count > 1)
		{
			target = touches[0];
			ApplyColor();
			for (int i = 1; i < touches.Count; i++)
			{
				Vector3 distance1 = touches[i - 1].transform.position - transform.position;
				Vector3 distance2 = touches[i].transform.position - transform.position;

				if (Vector3.SqrMagnitude(distance2) < Vector3.SqrMagnitude(distance1))
				{
					target = touches[i];
					ApplyColor();
					touches[i - 1].GetComponent<SpriteRenderer>().color = OriginalFrameColor;
				}
				else
				{
					touches[i].GetComponent<SpriteRenderer>().color = OriginalFrameColor;
				}
			}
		}
	}

	private void ApplyColor()
	{
		if (target.CanApplyCard(this))
		{
			target.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
		else
		{
			target.GetComponent<SpriteRenderer>().color = Color.red;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		touches.Add(collision.GetComponent<Frame>());

		if (touches.Count == 1)
		{
			target = collision.GetComponent<Frame>();
			ApplyColor();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		touches.Remove(collision.GetComponent<Frame>());
		collision.GetComponent<SpriteRenderer>().color = OriginalFrameColor;
	}

}