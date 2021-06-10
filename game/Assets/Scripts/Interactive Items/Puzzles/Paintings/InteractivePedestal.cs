using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractivePedestal : InteractiveItem
{
	[SerializeField] private string _infoText = null;

	// these are set from the PaintingsManager script
	[HideInInspector] public List<string> items = new List<string>();
	[HideInInspector] public string correctItem = "";

	private Collectable _coll = null;
	private InteractiveCollectable _intColl = null;
	private bool _isSolved = false;
	// did the player put a (wrong) item on it?
	private bool _isOccupied = false;

	public bool IsSolved { get { return _isSolved; } }

	private void Update()
	{
		// if an item is placed, check if it's still in place
		if (_intColl != null && _intColl.isPicked)
		{
			if (_isSolved)
				_isSolved = false;

			if (_isOccupied)
				_isOccupied = false;
		}
	}

	public override string GetText(CharacterManager cm)
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		if (_isSolved || _isOccupied)
			return;

		_coll = characterManager.SubtractCollectable(correctItem);
		// if you have the correct item in the inventory
		if (_coll != null)
		{
			_isSolved = true;
			PlaceItem(_coll);
		}
		// else just put a random wrong item
		else
		{
			// shuffle item list so that you search for a random order of items
			GameManager.Instance.Shuffle(items);
			// loop through it to check if you have at least one of those items to put on the pedestal
			for (int i = 0; i < items.Count; i++)
			{
				_coll = characterManager.SubtractCollectable(items[i]);
				if (_coll != null)
				{
					_isOccupied = true;
					PlaceItem(_coll);
					return;
				}
			}
			// if you have none, just output a negative feedback
			StartCoroutine(characterManager.PlayerHUD.SetEventText("You have no items to use", characterManager.PlayerHUD.eventColors[0]));
		}
	}

	private void PlaceItem(Collectable coll)
	{
		_intColl = coll.gameObject.GetComponent<InteractiveCollectable>();
		coll.gameObject.transform.position = transform.position + Vector3.up * 1.1f;
		coll.gameObject.SetActive(true);
	}
}
