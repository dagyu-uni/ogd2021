using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Powerup/Speed Increase")]
public class SpeedIncrease : PowerUp
{
	[SerializeField] private float staminaMultiplier = -0.5f;
	[SerializeField] private float walkingSpeedMultiplier = 0.1f;
	private float _oldStaminaRecovery;
	private float _oldWalkingSpeed;

	private float _startingJump;

	protected override void PowerUpEffects(CharacterManager charManager)
	{
		base.PowerUpEffects(charManager);
		PlayerController controller = charManager.Controller;
		_oldStaminaRecovery = controller.StaminaRecovery;
		_oldWalkingSpeed = controller.PlayerMovement.walkSpeed;
		controller.StaminaRecovery = _oldStaminaRecovery + (_oldStaminaRecovery * staminaMultiplier);
		Debug.Log(_oldWalkingSpeed);
		Debug.Log(_oldWalkingSpeed * walkingSpeedMultiplier);
		controller.PlayerMovement.walkSpeed = _oldWalkingSpeed + (_oldWalkingSpeed * walkingSpeedMultiplier);
	}

	protected override void RemoveEffects(CharacterManager charManager)
	{
		base.RemoveEffects(charManager);
		PlayerController controller = charManager.Controller;
		controller.StaminaRecovery = _oldStaminaRecovery;
		controller.PlayerMovement.walkSpeed = _oldWalkingSpeed;

	}
}
