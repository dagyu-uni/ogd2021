using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: this is just a parent class from which inherit specific skills script generators.
public abstract class Skill : ScriptableObject
{
	public string skillName = "New Skill";
	[SerializeField] private string _fireButton = "";
	public bool isBaseSkill = false;

	public Sprite sprite;
	public AudioCollection audioCollection;
	public float baseCooldown = 1f;
	public float currentCooldown = 0f;
	// duration stores the length of skill's effects
	public bool isOneShotSkill = true;
	public float baseDuration = 0f;
	public float currentDuration = 0f;

	[HideInInspector] public int uiPriority;

	protected CharacterManager _charManager = null;

	public string FireButton
	{
		get { return _fireButton; }
		//set { _fireButton = value; }
	}

	// used to save all the info you need to perform the skill
	public virtual void Initialize(CharacterManager charManager)
	{
		_charManager = charManager;
	}

	// apply skill's effects
	public virtual void TriggerSkill()
	{
		// has no duration involved
		if (isOneShotSkill)
			ActivateEffects();
		else
		{
			_charManager.CallCoroutine(DurationEffects());
		}
	}

	private IEnumerator DurationEffects()
	{
		ActivateEffects();
		yield return new WaitForSeconds(baseDuration);
		DeactivateEffects();
	}

	public abstract void ActivateEffects();
	public abstract void DeactivateEffects();
}
