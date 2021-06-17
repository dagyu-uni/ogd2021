using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemNames { Ring, Dice, GlobeMap, SkyrimMap };

[System.Serializable]
public class PaintingMaterials
{
	public string paintingName;
	public List<Material> itemMats = new List<Material>();
}

[System.Serializable]
public class PaintingPositions
{
	public string paintingName;
	public List<Transform> positions = new List<Transform>();
}

public class PaintingsManager : PuzzleManager
{
	// all the possible collectable items for this puzzle
	[SerializeField] private List<InteractiveCollectable> _collectables = new List<InteractiveCollectable>();
	[SerializeField] private List<InteractivePedestal> _pedestals = new List<InteractivePedestal>();
	// paintings base prefabs
	[SerializeField] private List<GameObject> _paintings = new List<GameObject>();
	// paintings materials
	[SerializeField] private List<PaintingMaterials> _paintingMaterials = new List<PaintingMaterials>();
	// Positions
	[SerializeField] private List<PaintingPositions> _paintingsPos = new List<PaintingPositions>();
	[SerializeField] private List<Transform> _itemsPos = new List<Transform>();

	private List<InteractiveCollectable> _correctItems = new List<InteractiveCollectable>();

	protected override void Start()
	{
		base.Start();

		// Set the pedestals
		List<CollectableName> names = new List<CollectableName>();
		for (int i = 0; i < _collectables.Count; i++)
		{
			names.Add(_collectables[i].Collectable);
		}

		// select three correct items and cache them
		List<int> range = Enumerable.Range(0, _collectables.Count).ToList<int>();
		for (int i = 0; i < 3; i++)
		{
			// choose the correct item
			int index = Random.Range(0, _collectables.Count - i);
			_correctItems.Add(_collectables[range[index]]);
			range.RemoveAt(index);
			// set the pedestal
			_pedestals[i].items = names;
			_pedestals[i].correctItem = _correctItems[i].Collectable;
			// set the painting
			int rand = Random.Range(0, 2);
			int rand2 = rand == 0 ? 1 : 0;
			// set base painting position and rotation
			GameObject truePainting = Instantiate(_paintings[i], _paintingsPos[i].positions[rand].position, _paintingsPos[i].positions[rand].rotation);
			GameObject fakePainting = Instantiate(_paintings[i], _paintingsPos[i].positions[rand2].position, _paintingsPos[i].positions[rand2].rotation);
			MeshRenderer fakeRenderer = fakePainting.GetComponent<MeshRenderer>();
			int matIndex = names.IndexOf(_correctItems[i].Collectable) * 2;
			int rendererIndex = fakeRenderer.materials.Length - 1;
			// note that all the arrays in unity, materials returns a copy of that array
			Material[] matArr = fakeRenderer.materials;
			matArr[rendererIndex] = _paintingMaterials[i].itemMats[Random.Range(matIndex, matIndex + 1)];
			fakeRenderer.materials = matArr;
		}

		// Set the items random position
		List<int> itemRange = Enumerable.Range(0, _itemsPos.Count).ToList<int>();
		for (int i = 0; i < _collectables.Count; i++)
		{
			int index = Random.Range(0, _itemsPos.Count - i);
			Transform pos = _itemsPos[itemRange[index]];
			_collectables[i].transform.position = pos.position;
			_collectables[i].transform.rotation = pos.rotation;
			itemRange.RemoveAt(index);

			if (PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.Instantiate(_collectables[i].name,
					_collectables[i].transform.position, _collectables[i].transform.rotation);
			}
		}
	}

	public override bool IsPuzzleSolved()
	{
		for (int i = 0; i < _pedestals.Count; i++)
		{
			if (!_pedestals[i].IsSolved)
			{
				return false;
			}
		}

		return true;
	}

	public override void ActivateReward()
	{
		base.ActivateReward();
		Debug.Log("Paintings Solved!");
	}
}
