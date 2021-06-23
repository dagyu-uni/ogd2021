using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MirrorsManager : PuzzleManager
{
	public List<Transform> _mirrorTrans = new List<Transform>();

	[SerializeField] private List<MirrorHandler> _mirrors = new List<MirrorHandler>();
	[SerializeField] private List<InteractiveHead> _heads = new List<InteractiveHead>();

	private List<int> _indices = new List<int>();

	protected override void Start()
	{
		List<int> range = Enumerable.Range(0, _mirrors.Count).ToList<int>();

		for (int i = 0; i < _mirrors.Count; i++)
		{
			// Spawn mirrors at random on fixed positions
			int j = Random.Range(0, _mirrors.Count - i);
			_mirrors[i].transform.position = _mirrorTrans[range[j]].position;
			_mirrors[i].transform.rotation = _mirrorTrans[range[j]].rotation;
			range.RemoveAt(j);

			// set correct rotation
			int index = Random.Range(0, 4);
			_indices.Add(index);
			_mirrors[i].SetRotation(index);
		}
	}

	public override bool IsPuzzleSolved()
	{
		bool res = true;

		for (int i = 0; i < _heads.Count; i++)
		{
			res = (_heads[i].correctRot / 90) == _indices[i];

			if (!res)
				break;
		}

		return res;
	}

	public override void ActivateReward()
	{
		base.ActivateReward();
		// unlock melody puzzle
		Debug.Log("MIRRORS SOLVED");
	}
}
