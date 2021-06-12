using UnityEngine;


[CreateAssetMenu(menuName = "Skill/Silence")]
public class Silence : Skill
{
	public override void ActivateEffects()
	{
		_charManager.Controller.AreFootprintActive = false;
	}

	public override void DeactivateEffects()
	{
		_charManager.Controller.AreFootprintActive = true;
	}
}
