using UnityEngine;

/// <summary>
/// The character controller class to handle inputs.
/// </summary>
[RequireComponent(typeof (CharacterMotor))]
public class PlayerController : MonoBehaviour {

	/// <summary>
	/// A reference to the character's CharacterMotor component.
	/// </summary>
	private CharacterMotor _character;

	/// <summary>
	/// Unity MonoBehaviour's Awake.
	/// </summary>
	private void Awake()
	{
		_character = GetComponent<CharacterMotor>();
	}

	/// <summary>
	/// Unity MonoBehaviour's Update.
	/// </summary>
	void Update ()
	{
		//
		// Fast implementation for testing purpuses
		//
		if(Input.GetButtonDown("Jump"))
		{
			_character.Action ();
		}

		if (Input.GetButtonDown("Fire1"))
		{
			_character.Run();
		}
		if (Input.GetButtonUp("Fire1"))
		{
			_character.Walk();
		}

		_character.setHorizontalSpeed(Input.GetAxis("Horizontal"));
	}
}