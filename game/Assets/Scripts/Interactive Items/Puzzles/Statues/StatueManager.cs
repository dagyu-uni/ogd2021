using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatueManager : PuzzleManager
{
	[SerializeField] private List<InteractiveStatue> _statues = new List<InteractiveStatue>();
	[SerializeField] private List<GameObject> _clues = new List<GameObject>();
	[SerializeField] private List<Transform> _cluesPositions = new List<Transform>();
	[SerializeField] private CompassImage _compassImage = null;
	[SerializeField] private GameObject _compass = null;
	[SerializeField] private List<Transform> _compassPositions = new List<Transform>();

	private string[] _cardinals = { "North", "North-East", "East", "South-East", "South", "South-West", "West", "North-West" };
	private List<int> _correctOrientations = new List<int>();
	// represents the cardinal orientation of the world.
	private float _offset;
	protected override void Start()
	{
		base.Start();
		// Get world cardinal orientation and set compass UI image offset
		int r = Random.Range(0, 8);
		_offset = r * 45f;
		_compassImage.Offset = _offset;

		if (PhotonNetwork.IsMasterClient)
		{
			int ci = Random.Range(0, 3);
			PhotonNetwork.Instantiate(_compass.name,
					_compassPositions[ci].transform.position, _compassPositions[ci].transform.rotation);
		}

		// Set correct clues text and statue correct orientations
		List<int> range = Enumerable.Range(0, _cluesPositions.Count).ToList<int>();
		for (int i = 0; i < _statues.Count; i++)
		{
			int rand = Random.Range(0, 8);
			_correctOrientations.Add(rand * 45);

			// Set clue position (randomly)
			int index = Random.Range(0, _cluesPositions.Count - i);
			_clues[i].transform.position = _cluesPositions[range[index]].position;
			range.RemoveAt(index);
			// Set clue text
			Text _clueText = _clues[i].GetComponentInChildren(typeof(Text), true) as Text;
			_clueText.text = "The statue " + (i + 1) + " must point " + _cardinals[(rand + r) % 8];

			Instantiate(_clues[i], _clues[i].transform.position, _clues[i].transform.rotation);
		}
	}

	public override bool IsPuzzleSolved()
	{
		for (int i = 0; i < _statues.Count; i++)
		{
			int statueOrientation = (int)_statues[i].transform.rotation.eulerAngles.y + _statues[i].offset;
			if (statueOrientation != _correctOrientations[i])
			{
				return false;
			}
		}

		return true;
	}

	public override void ActivateReward()
	{
		base.ActivateReward();
		// TODO do something
		Debug.Log("Statue Puzzle Solved!");
	}
}
