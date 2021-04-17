using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Powerup/Super Jump")]
public class SuperJump : PowerUp
{
	[SerializeField] private float _jumpForce = 20.0f;

	private float _startingJump;

	protected override void PowerUpEffects(CharacterManager charManager)
	{
		base.PowerUpEffects(charManager);
		_startingJump = charManager.Movement.JumpSpeed;
		charManager.Movement.JumpSpeed = _jumpForce;
	}

	protected override void RemoveEffects(CharacterManager charManager)
	{
		base.RemoveEffects(charManager);
		charManager.Movement.JumpSpeed = _startingJump;
	}
}
