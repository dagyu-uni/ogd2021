using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Invisibility")]
public class Invisibility : Skill
{
	public Material invisibleMaterial = null;
	[HideInInspector] public List<Material> _originalMats = new List<Material>();

	private SkinnedMeshRenderer[] _renderers;


	public override void Initialize(CharacterManager charManager)
	{
		base.Initialize(charManager);
		_renderers = charManager.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		_originalMats = new List<Material>();
	}

	public override void ActivateEffects()
	{
		// RPC
		GameManager.Instance.Invisibility(_charManager.Role, true);

		if (_originalMats.Count == 0)
		{
			for (int i = 0; i < _renderers.Length; i++)
			{
				_originalMats.Add(_renderers[i].material);
			}
		}

		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].material = invisibleMaterial;
		}
	}

	public override void DeactivateEffects()
	{
		// RPC
		GameManager.Instance.Invisibility(_charManager.Role, false);

		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].material = _originalMats[i];
		}
	}
}
