using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Transformation")]
public class Transformation : Skill
{
	// List of gameobjects usable for transformation
	public List<GameObject> _objects = new List<GameObject>();

	private SkinnedMeshRenderer _renderer = null;

	public override void Initialize(CharacterManager charManager)
	{
		base.Initialize(charManager);
		_renderer = charManager.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
	}

	public override void ActivateEffects()
	{
		GameObject prop = _objects[Random.Range(0, _objects.Count)];

		// TODO play a particle effect here

		_renderer.enabled = false;
		Instantiate(prop, _charManager.transform.position, _charManager.transform.rotation);
	}

	public override void DeactivateEffects()
	{
		// TODO play a particle effect here

		_renderer.enabled = true;
	}
}
