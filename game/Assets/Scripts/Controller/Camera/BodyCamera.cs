using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script changes the position and clamping rotation of
// the player secondary camera, used to render only their body.
// This allows the player to always see his body while not seeing
// any visible artifact.
public class BodyCamera : MonoBehaviour
{
	[SerializeField] private PlayerController _playerController = null;

	public float lerpSpeed = 10f;

	private Vector3 _idlePos;
	private Vector3 _walkingPos;
	private Vector3 _runningPos;
	private Vector3 _idleCrouchPos;
	private Vector3 _movingCrouchPos;
	private Vector3 _idleJump;
	private Vector3 _movingJump;
	private Vector3 _idleFall;

	private Coroutine _coroutine = null;

	void Start()
	{
		_idlePos = new Vector3(0f, 0.5f, 0.15f);
		_walkingPos = new Vector3(0f, 0.5f, 0.3f);
		_runningPos = new Vector3(0f, 0.67f, 0.42f);
		_idleCrouchPos = new Vector3(0f, 0.93f, 0.25f);
		_movingCrouchPos = new Vector3(0f, 1.1f, 0.5f);
		_idleJump = new Vector3(0f, 0.0f, 0.38f);
		_movingJump = new Vector3(0f, 0.4f, 0.3f);
		_idleFall = new Vector3(0f, -0.3f, 0.7f);
	}


	void Update()
	{
		if (_coroutine != null)
			return;

		// Landing after fall (not after jumping)
		if (_playerController.IsLanding)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _idleFall, Time.deltaTime * lerpSpeed * 100f);
			_coroutine = StartCoroutine(WaitFallingAnimation());
		}
		// walking
		if (_playerController.status == Status.walking && !_playerController.IsJumping)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _walkingPos, Time.deltaTime * lerpSpeed);
		}
		// sprinting
		else if (_playerController.status == Status.sprinting && !_playerController.IsJumping)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _runningPos, Time.deltaTime * lerpSpeed);
		}
		// idle crouching
		else if (_playerController.status == Status.crouching
					&& Input.GetAxisRaw("Horizontal") == 0f && Input.GetAxisRaw("Vertical") == 0f)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _idleCrouchPos, Time.deltaTime * lerpSpeed);
		}
		// moving crouching
		else if (_playerController.status == Status.crouching
					&& (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f))
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _movingCrouchPos, Time.deltaTime * lerpSpeed);
		}
		// Idle Jump
		else if (_playerController.IsJumping
					&& Input.GetAxisRaw("Horizontal") == 0f && Input.GetAxisRaw("Vertical") == 0f)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _idleJump, Time.deltaTime * lerpSpeed);
		}
		// Moving Jump
		else if (_playerController.IsJumping
					&& (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f))
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _movingJump, Time.deltaTime * lerpSpeed);
		}
		// idle
		else
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, _idlePos, Time.deltaTime * lerpSpeed);
		}
	}

	private IEnumerator WaitFallingAnimation()
	{
		yield return new WaitForSeconds(0.2f);
		_coroutine = null;
	}
}
