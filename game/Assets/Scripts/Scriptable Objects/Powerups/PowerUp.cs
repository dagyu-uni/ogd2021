using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: this is just a parent class from which inherit specific power-ups script generators
// use this as generator if you only need to reference a skill and nothing else.

// NOTE 2: the isGlobal property only supports powerups (passive skills) not active skills.
[CreateAssetMenu(menuName = "Powerup/Skill only")]
public class PowerUp : ScriptableObject
{
	// used to give the power-up to all players of a specific role
	[SerializeField] private bool _isGlobal = false;
	// who collects this item learns this skill
	[SerializeField] private Skill _unlockedSkill = null;

	public bool HasSkill { get { return _unlockedSkill != null; } }

	public void ApplyPowerUp(CharacterManager charManager)
	{
		if (!_isGlobal)
			PowerUpEffects(charManager);
		else
		{
			foreach (PlayerInfo info in GameManager.Instance.PlayersInfo.Values)
			{
				if (info.role > 0)
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
				if (info.role > 0)
					RemoveEffects(info.characterManager);
			}
		}
	}

	// actual methods to override to describe specifics effects of the power-up.
	protected virtual void PowerUpEffects(CharacterManager charManager)
	{
		if (_unlockedSkill != null && !_isGlobal)
		{
			_unlockedSkill.uiPriority = charManager.Inventory[charManager.Inventory.Count - 1].uiPriority;
			charManager.Skills.Add(_unlockedSkill);
			charManager.Skills.Sort((a, b) => b.uiPriority.CompareTo(a.uiPriority));
			_unlockedSkill.Initialize(charManager);
			_unlockedSkill.currentDuration = 0f;
			_unlockedSkill.currentCooldown = _unlockedSkill.baseCooldown;
			charManager.PlayerHUD.InitiliazeSkillSlots();
		}
	}
	protected virtual void RemoveEffects(CharacterManager charManager)
	{
		if (_unlockedSkill != null && !_isGlobal)
		{
			for (int i = 0; i < charManager.Skills.Count; i++)
			{
				Skill sk = charManager.Skills[i];
				if (sk.name == _unlockedSkill.name)
				{
					charManager.Skills.RemoveAt(i);
					charManager.PlayerHUD.InitiliazeSkillSlots();
				}
			}
		}
	}
}
