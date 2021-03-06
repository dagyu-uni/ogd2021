using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveCylinder : InteractiveItem
{
	[SerializeField] private CollectableName _treasureName;

	public Collectable coll = null;

	public override string GetText(CharacterManager cm)
	{
		return "Place " + _treasureName;
	}

	public override void Activate(CharacterManager characterManager)
	{
		coll = characterManager.SubtractCollectable(_treasureName);
		if (coll != null)
		{
			coll.gameObject.transform.position = transform.position + Vector3.up;
			coll.gameObject.SetActive(true);
		}
	}
}
