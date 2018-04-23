using System;
using UnityEngine;

/// <summary>
/// The character motor class. It controls the player behaviour, movement, jumps, powerup events, etc.
/// </summary>
[RequireComponent(typeof (Rigidbody2D))]
[RequireComponent(typeof (Animator))]
public class CharacterMotor : MonoBehaviour
{
	public float maxHorizontalSpeed = 10f;
	public float runSpeedMultiplier = 2f;
	private float _targetSpeedMultiplier = 1f;

	public float runLerpTime = 0.4f;
	public float walkLerpTime = 0.3f;

	private bool _runLerp = false;
	private bool _walkLerp = false;

	public float _timeToPeak = 0.35f;
	public float _jumpHeight = 5f;
	public float _wallSpeed = 5f;
	private float _usedSpeedMultiplier = 1;

	public Action OnEnterDeathTrap;
	public Action<MapLayout> OnLeftLayout;
	public Action OnFinishGame;

	[SerializeField] private LayerMask _whatIsGround;

	[SerializeField] private Vector2 _groundCheckOffset = new Vector2(0, 0);
	[SerializeField] private Vector2 _groundCheckBox = new Vector2 (0.9f, 0.2f);

	[SerializeField] private Vector2 _wallCheckOffset = new Vector2 (0f, 0.6f);
	[SerializeField] private Vector2 _wallCheckBox = new Vector2 (1.2f, 0.1f);

	private Rigidbody2D _rigidbody2D;
	private Animator _animator;

	private float _gravity;
	private float _jumpVelocity;
	private float _usedHorizontalSpeed;
	private float _targetHorizontalSpeed;

	private bool _grounded;
	private bool _walled;
	private bool _facingRight = true;
	private bool _airJump = false;

	private bool _lerpSpeed = false;
	private float _lerpStartTime;
	private float _lerpDuration;

	public float _wallJumpForce = 10f;
	public float _wallJumpForceDuration = 0.5f;

	private bool _wallStick = false;
	private float _wallStickStartTime;
	public float _wallStickDuration = 0.1f;
		
	public float _wallUnstickForce = 3f;
	public float _wallUnstickForceDuration = 0.5f;

	public bool canDoubleJump = true;
	private bool _canRun = true;

	public LerpColor denySprite;

	/// <summary>
	/// Unity MonoBehaviour's Awake.
	/// </summary>
	private void Awake()
	{
		_rigidbody2D = GetComponent<Rigidbody2D>();
		_animator = GetComponent<Animator> ();
			
		//
		// Defines the jumpVecolity and gravity to achieve the desired jump height and time spent on air.
		//
		_jumpVelocity = (2f * _jumpHeight) / _timeToPeak;
		_gravity = -(2f * _jumpHeight) / (_timeToPeak * _timeToPeak);
		_rigidbody2D.gravityScale = (_gravity) / Physics2D.gravity.y ;

		//
		// Sets the actual horizontal speed variable used
		//
		_usedHorizontalSpeed = 0;
	}

	public void RecalculateGravity()
	{
		_jumpVelocity = (2f * _jumpHeight) / _timeToPeak;
		_gravity = -(2f * _jumpHeight) / (_timeToPeak * _timeToPeak);
		_rigidbody2D.gravityScale = (_gravity) / Physics2D.gravity.y;
	}

	/// <summary>
	/// Unity MonoBehaviour's AwaOnDrawGizmos event.
	/// </summary>
	private void OnDrawGizmos()
	{
		//
		// Debugs the ground check collision
		//
		if (_grounded)
		{
			Gizmos.color = new Color (0f, 1f, 0f, 0.2f);
		}
		else
		{
			Gizmos.color = new Color (1f, 0f, 0f, 0.2f);
		}
		Gizmos.DrawCube ((Vector2)transform.position + _groundCheckOffset, _groundCheckBox);

		//
		// Debugs the walled check collision
		//
		if (_walled)
		{
			Gizmos.color = new Color (0f, 1f, 0f, 0.2f);
		}
		else
		{
			Gizmos.color = new Color (1f, 0f, 0f, 0.2f);
		}
		Gizmos.DrawCube ((Vector2)transform.position + _wallCheckOffset, _wallCheckBox);
	}

	/// <summary>
	/// Unity MonoBehaviour's FixedUpdate.
	/// </summary>
	private void FixedUpdate()
	{
		//
		// Resets the conditions
		//
		_grounded = false;
		_walled = false;
		_rigidbody2D.gravityScale = (_gravity) / Physics2D.gravity.y ;

		//
		// Sets the correct LayerMask for collistion
		//
		LayerMask usedLayerMask = _whatIsGround;
		
		//
		// Check for ground collisions and set variables acordingly
		//
		Collider2D[] colliders = Physics2D.OverlapBoxAll ((Vector2)transform.position + _groundCheckOffset, _groundCheckBox, 0f, usedLayerMask);

		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders [i].gameObject != gameObject)
			{
				_grounded = true;
				_airJump = false;
			}
		}

		//
		// Check for walls collisions and set variables acordingly
		//
		colliders = Physics2D.OverlapBoxAll ((Vector2)transform.position + _wallCheckOffset, _wallCheckBox, 0f, usedLayerMask);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders [i].gameObject != gameObject && Mathf.Abs(_rigidbody2D.velocity.x) < 0.001f)
			{
				_walled = true;
				_airJump = false;

				bool rightWall = Physics2D.Raycast(transform.position, Vector2.right, 1f, usedLayerMask);

				if (rightWall)
				{
					if (!_facingRight)
					{
						Flip();
					}
				}
				else if(_facingRight)
				{
					// There's still a glitch when facing right and walling and you press left
					Flip();
				}
			}
		}

		//
		// Controls the fall velocity in case the player is in the air and touching the wall
		//
		float yVelocity = _rigidbody2D.velocity.y;
		if (_walled)
		{
			if (yVelocity < 0)
			{
				_rigidbody2D.gravityScale = 0;

				yVelocity = -_wallSpeed;
			}
			else if (yVelocity == 0)
			{
				//
				// In the corner
				//
			}
		}

		if (_wallStick)
		{
			if(_grounded)
			{
				_wallStick = false;
				_lerpSpeed = false;
			}
			else
			{
				if (Time.time - _wallStickStartTime >= _wallStickDuration)
				{
					_wallStick = false;

					_targetHorizontalSpeed = _facingRight ? -_wallUnstickForce : _wallUnstickForce;
					_lerpSpeed = true;
					_lerpStartTime = Time.time;
					_lerpDuration = _wallUnstickForceDuration;
				}
				else
				{
					_usedHorizontalSpeed = 0;
				}
			}
		}

		//
		// Controls the direction the player is facing
		//
		if (_usedHorizontalSpeed > 0 && !_facingRight || _usedHorizontalSpeed < 0 && _facingRight)
		{
			// ... flip the player.
			Flip();
		}

		//
		// Move the character using the current horizontal speed
		//
		if (_lerpSpeed)
		{
			float timePassed = Time.time - _lerpStartTime;

			_usedHorizontalSpeed = Mathf.Lerp(_targetHorizontalSpeed, _usedHorizontalSpeed, timePassed / _lerpDuration);

			if(timePassed >= _lerpDuration)
			{
				_lerpSpeed = false;
			}
		}

		if(_runLerp)
		{
			float total = _usedSpeedMultiplier / _targetSpeedMultiplier - 0.5f;
			_usedSpeedMultiplier = Mathf.Lerp(_usedSpeedMultiplier, _targetSpeedMultiplier, total + (Time.deltaTime * runLerpTime));
			if(_usedSpeedMultiplier >= _targetSpeedMultiplier)
			{
				_usedSpeedMultiplier = _targetSpeedMultiplier;
				_runLerp = false;
			}
		}

		if (_walkLerp)
		{
			float total = _targetSpeedMultiplier / _usedSpeedMultiplier - 0.5f;
			_usedSpeedMultiplier = Mathf.Lerp(_usedSpeedMultiplier, _targetSpeedMultiplier, total + (Time.deltaTime * walkLerpTime));
			if (_usedSpeedMultiplier <= _targetSpeedMultiplier)
			{
				_usedSpeedMultiplier = _targetSpeedMultiplier;
				_walkLerp = false;
			}
		}

		_rigidbody2D.velocity = new Vector2(_usedHorizontalSpeed * _usedSpeedMultiplier, yVelocity);
			
		//
		// Control all animations
		//
		if (_grounded)
		{
			if (slideAudioSource.isPlaying)
			{
				slideAudioSource.Stop();
			}

			if (_usedHorizontalSpeed == 0f)
			{
				_animator.SetInteger("AnimState", 4);
			}
			else
			{
				_animator.SetInteger("AnimState", 0);
			}
		}
		else
		{
			if (_walled)
			{
				_animator.SetInteger ("AnimState", 3);

				if(!slideAudioSource.isPlaying)
				{
					slideAudioSource.Play();
				}
			}
			else
			{
				if (slideAudioSource.isPlaying)
				{
					slideAudioSource.Stop();
				}

				if (yVelocity > 0)
				{
					_animator.SetInteger ("AnimState", 1);
				}
				else
				{
					_animator.SetInteger ("AnimState", 2);
				}
			}
		}
	}

	/// <summary>
	/// Sets the player horizontal speed.
	/// </summary>
	/// <param name="horizontalSpeed">Horizontal speed.</param>
	public void setHorizontalSpeed(float horizontalSpeed)
	{
		if (_walled && !_grounded && !_wallStick && !_lerpSpeed &&
			((_facingRight && horizontalSpeed < 0)
			|| (!_facingRight && horizontalSpeed > 0)))
		{
			_wallStick = true;
			_wallStickStartTime = Time.time;
		}
		
		_usedHorizontalSpeed = horizontalSpeed * maxHorizontalSpeed;
	}

	public void Action()
	{
		//
		// If grounded, jump
		//
		if (_grounded)
		{
			//
			// Add a vertical force to the player.
			//
			PlayRandomSound(jumps);
			_rigidbody2D.velocity = new Vector2 (_rigidbody2D.velocity.x, _jumpVelocity);
			_grounded = false;
		}
		//
		// If aired, didn't double jump yet and can use double jump
		//
		else if (!_airJump)
		{		
			if (!_walled)
			{
				if (!canDoubleJump)
				{
					PlayRandomSound(deny);
					denySprite.StartLerp();
					return;
				}
				
				_airJump = true;
			}
			else
			{
				_wallStick = false;
				_targetHorizontalSpeed = _facingRight ? -_wallJumpForce : _wallJumpForce;
				_lerpSpeed = true;
				_lerpStartTime = Time.time;
				_lerpDuration = _wallJumpForceDuration;
			}

			PlayRandomSound(jumps);
			_rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpVelocity);
		}
	}

	public void SetCanRun(bool canRun)
	{
		this._canRun = canRun;

		if(!canRun)
		{
			Walk();
		}
	}

	public void Run()
	{
		if (!_canRun)
		{
			PlayRandomSound(deny);
			denySprite.StartLerp();
			return;
		}

		_targetSpeedMultiplier = runSpeedMultiplier;
		_runLerp = true;
	}

	public void Walk()
	{
		_targetSpeedMultiplier = 1;
		_walkLerp = true;
	}
	
	/// <summary>
	/// Resets the character motion.
	/// </summary>
	public void ResetCharacterMotion()
	{
		_rigidbody2D.isKinematic = false;
		_grounded = true;
		_walled = false;
		_airJump = false;
		_runLerp = false;
		_walkLerp = false;
		_wallStick = false;
		_lerpSpeed = false;
		_usedHorizontalSpeed = 0f;
		_usedSpeedMultiplier = 1f;
	}

	/// <summary>
	/// Flip the player sprite.
	/// </summary>
	private void Flip()
	{
		//
		// Switch the way the player is labelled as facing.
		//
		_facingRight = !_facingRight;

		//
		// Multiply the player's x local scale by -1.
		//
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
	
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.CompareTag("Deathtrap"))
		{
			if(OnEnterDeathTrap != null)
			{
				OnEnterDeathTrap.Invoke();
			}

			PlayRandomSound(death);

			return;
		}

		if(collision.CompareTag("Exit"))
		{
			if (OnFinishGame != null)
			{
				OnFinishGame.Invoke();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Layout"))
		{
			if (OnLeftLayout != null)
			{
				OnLeftLayout.Invoke(collision.GetComponent<MapLayout>());
			}
		}
	}

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	AudioClip[] step;

	[SerializeField]
	AudioClip[] jumps;

	[SerializeField]
	AudioClip[] death;

	[SerializeField]
	private AudioSource slideAudioSource;

	[SerializeField]
	AudioClip[] deny;

	public void PlayStep()
	{
		PlayRandomSound(step);
	}

	private void PlayRandomSound(AudioClip[] clips)
	{
		audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
	}
}