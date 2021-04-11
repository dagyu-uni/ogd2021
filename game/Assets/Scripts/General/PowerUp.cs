using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: this is just a parent class from which inherit specific power-ups.
public abstract class PowerUp : MonoBehaviour
{
	// used to give the power-up to all players of a specific role
	[SerializeField] private bool _isGlobal = false;

	public void ApplyPowerUp(CharacterManager charManager)
	{
		if (!_isGlobal)
			PowerUpEffects(charManager);
		else
		{
			foreach (PlayerInfo info in GameManager.Instance.PlayersInfo.Values)
			{
				if (info.role == Role.Wizard)
					PowerUpEffects(info.characterManager);
			}
		}		
	}

	public void RemovePowerUp(CharacterManager charManager)
	{
		if (!_isGlobal)
			RemoveEffects(charManager);
		else
		{
			foreach (PlayerInfo info in GameManager.Instance.PlayersInfo.Values)
			{
				if (info.role == Role.Wizard)
					RemoveEffects(info.characterManager);
			}
		}
	}

	// actual method to override to describe specifics effects of the power-up.
	protected abstract void PowerUpEffects(CharacterManager charManager);
	protected abstract void RemoveEffects(CharacterManager charManager);
}
