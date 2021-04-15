using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveCube : InteractiveItem
{
	[SerializeField] private string _infoText = null;
	[SerializeField] private List<Material> _colors = null;

	private MeshRenderer _renderer = null;
	private int index = 0;

	private void Awake()
	{
		_renderer = GetComponent<MeshRenderer>();
	}

	public override string GetText()
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		_renderer.material = _colors[index];
		index = index + 1 >= _colors.Count ? 0 : index + 1;
	}
}
