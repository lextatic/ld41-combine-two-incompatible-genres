using UnityEngine;

public class Saw : MonoBehaviour
{
	public Transform saw;
	public Transform position1;
	public Transform position2;

	private Vector3 targetPosition;
	private bool swapToPosition1;

	public float sawSpeed = 4f;

	private void Start()
	{
		targetPosition = position2.position;
		swapToPosition1 = true;
	}

	private void FixedUpdate()
	{
		saw.Rotate(0, 0, 980 * Time.deltaTime, Space.Self);
		saw.Translate((targetPosition - saw.transform.position).normalized * sawSpeed * Time.deltaTime, Space.World);

		if(Vector3.Distance(saw.position, targetPosition) < 0.1f)
		{
			swapToPosition1 = !swapToPosition1;
			if (swapToPosition1)
			{
				targetPosition = position1.position;
			}
			else
			{
				targetPosition = position2.position;
			}
		}
	}

	public void TranslateSaw(Vector3 translationValue)
	{
		targetPosition = targetPosition + translationValue;
	}
}
