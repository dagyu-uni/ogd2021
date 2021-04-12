using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperJump : PowerUp
{
	[SerializeField] private float _jumpForce = 20.0f;

	private float _startingJump;

	protected override void PowerUpEffects(CharacterManager charManager)
	{
		_startingJump = charManager.Movement.JumpSpeed;
		charManager.Movement.JumpSpeed = _jumpForce;
	}

	protected override void RemoveEffects(CharacterManager charManager)
	{
		charManager.Movement.JumpSpeed = _startingJump;
	}
}
