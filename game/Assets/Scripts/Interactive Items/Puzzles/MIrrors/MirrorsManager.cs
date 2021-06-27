using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MirrorsManager : PuzzleManager, Randomizer
{
	public List<Transform> _mirrorTrans = new List<Transform>();

	[SerializeField] private List<MirrorHandler> _mirrors = new List<MirrorHandler>();
	[SerializeField] private List<InteractiveHead> _heads = new List<InteractiveHead>();

	private List<int> _indices = new List<int>();

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

	public void InitRandom()
	{
		List<int> range = Enumerable.Range(0, _mirrors.Count).ToList<int>();
		GameManager.Instance.Shuffle(range);

		for (int i = 0; i < _mirrors.Count; i++)
		{
			Debug.Log(i + " & " + range[i]);
			// Spawn mirrors at random on fixed positions
			_mirrors[i].transform.position = _mirrorTrans[range[i]].position;
			_mirrors[i].transform.rotation = _mirrorTrans[range[i]].rotation;

			// set correct rotation
			int index = Random.Range(0, 4);
			_indices.Add(index);
			_mirrors[i].SetRotation(index);
		}
	}
}
