using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderManager : PuzzleManager
{
	[SerializeField] private InteractiveCylinder _cylinder = null;
	[SerializeField] private GameObject _wall = null;

	public override bool IsPuzzleSolved()
	{
		return _cylinder.coll != null;
	}

	public override void ActivateReward()
	{
		base.ActivateReward();
		_wall.SetActive(false);
	}
}
