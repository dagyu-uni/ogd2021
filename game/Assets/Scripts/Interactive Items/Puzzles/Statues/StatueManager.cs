using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class StatueManager : PuzzleManager, Randomizer
{
	[SerializeField] private List<InteractiveStatue> _statues = new List<InteractiveStatue>();
	[SerializeField] private List<GameObject> _clues = new List<GameObject>();
	[SerializeField] private List<Transform> _cluesPositions = new List<Transform>();
	[SerializeField] private CompassImage _compassImage = null;
	[SerializeField] private GameObject _compass = null;
	[SerializeField] private List<Transform> _compassPositions = new List<Transform>();

	private string[] _cardinals = { "North", "North-East", "East", "South-East", "South", "South-West", "West", "North-West" };
	// in order, venere, Hermes, discobolo, Apollo
	private string[] _sentences = { "Even if they broke my body, they can't ruin my beauty! Let me look ",  "Between all the athletes, I'm by far the fastest. Let me fly towards " ,
									"If I want to win I have to throw my disc ", "Leader of muses and poetry, you should know my name. I may reveal your future if you turn me towards "};

	private List<int> _correctOrientations = new List<int>();
	// represents the cardinal orientation of the world.
	private float _offset;
	protected override void Start()
	{
		base.Start();
	}

	public override bool IsPuzzleSolved()
	{
		for (int i = 0; i < _statues.Count; i++)
		{

			int statueOrientation = (int)_statues[i].currentOrientation;
			Debug.Log("index " + i + " --- orientazione" + statueOrientation + " " + _correctOrientations[i]);
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

	public void InitRandom()
	{
		// Get world cardinal orientation and set compass UI image offset
		int r = Random.Range(0, 8);
		_offset = r * 45f;
		_compassImage.Offset = _offset;

		int ci = Random.Range(0, 3);
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.Instantiate(_compass.name,
			_compassPositions[ci].transform.position, _compassPositions[ci].transform.rotation);
		}

		// Set correct clues text and statue correct orientations
		List<int> range = Enumerable.Range(0, _cluesPositions.Count).ToList<int>();
		for (int i = 0; i < _statues.Count; i++)
		{
			int rand = Random.Range(0, 8);
			_correctOrientations.Add(rand);

			// Start with a random orientation
			float orientation = Random.Range(0, 8);
			Debug.Log(i + " OFF:" + r + "  OR: " + orientation);
			float normalizeOrientation = ((orientation + 8) - r) % 8;
			_statues[i].transform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + (normalizeOrientation * 45f), Vector3.up);
			_statues[i].currentOrientation = normalizeOrientation;

			// Set clue position (randomly)
			int index = Random.Range(0, _cluesPositions.Count - i);
			_clues[i].transform.position = _cluesPositions[range[index]].position;
			range.RemoveAt(index);
			// Set clue text
			Text _clueText = _clues[i].GetComponentInChildren(typeof(Text), true) as Text;
			_clueText.text = _sentences[i] + _cardinals[rand];

			Debug.Log("ST" + i + " O: " + _statues[i].currentOrientation + " CO: " + (rand * 45) + " SENT " + _clueText.text);

			Instantiate(_clues[i], _clues[i].transform.position, _clues[i].transform.rotation);
		}
	}
}
