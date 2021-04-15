using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CubesManager : PuzzleManager
{
	[SerializeField] private List<MeshRenderer> _cubes = new List<MeshRenderer>();
	[SerializeField] private List<Material> _solvingMaterials = new List<Material>();

	public override bool IsPuzzleSolved()
	{
		
		for (int i = 0; i < _cubes.Count; i++)
		{
			if (_cubes[i].material.name != _solvingMaterials[i].name + " (Instance)")
				return false;
		}

		return true;
	}

	public override void ActivateReward()
	{
		base.ActivateReward();
		_puzzleCollectable.isPickable = true;
	}

	public override void DeactivateReward()
	{
		_puzzleCollectable.isPickable = false;
	}
}
