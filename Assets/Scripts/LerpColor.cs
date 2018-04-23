using UnityEngine;

public class LerpColor : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;
	public float LerpDuration;

	private Color white = new Color(1, 1, 1, 0.5f);
	private Color transparent = new Color(1, 1, 1, 0);

	private float _lerpStartTime;

	public void StartLerp()
	{
		spriteRenderer.color = white;
		_lerpStartTime = Time.time;
	}

	private void Update()
	{
		if(Time.time - _lerpStartTime < LerpDuration)
		{
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, transparent, Time.deltaTime * LerpDuration);
		}
		else
		{
			spriteRenderer.color = transparent;
		}
	}
}
