using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Super Speed")]
public class SuperSpeedSkill : Skill
{
	public float superSpeed;

	private float _previousSpeed;

	public override void ActivateEffects()
	{
		_previousSpeed = _charManager.Movement.runSpeed;
		_charManager.Movement.runSpeed = superSpeed;
	}

	public override void DeactivateEffects()
	{
		_charManager.Movement.runSpeed = _previousSpeed;
	}
}
