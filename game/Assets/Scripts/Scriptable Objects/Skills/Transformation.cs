using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(menuName = "Skill/Transformation")]
public class Transformation : Skill
{
	// List of gameobjects usable for transformation
	public List<GameObject> _objects = new List<GameObject>();
	public List<float> heightsFactor = new List<float>();


	private SkinnedMeshRenderer _renderer = null;
	private GameObject _currentProp = null;
	private IEnumerator _coroutine = null;

	public override void Initialize(CharacterManager charManager)
	{
		base.Initialize(charManager);
		_renderer = charManager.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		_currentProp = null;
		_coroutine = CheckMovement();
	}

	public override void ActivateEffects()
	{
		_hasToDeactivate = true;
		int rand = Random.Range(0, _objects.Count);

		// particle effect
		GameManager.Instance.GeneratePropEffect();
		GameManager.Instance.GeneratePropObject(_objects[rand].name, heightsFactor[rand]);

		// handle meshes and cameras
		_renderer.enabled = false;
		_charManager.Camera.gameObject.SetActive(false);
		_charManager.DisableControllerMovements();
		Vector3 pos = _charManager.transform.position + Vector3.down * heightsFactor[rand];
		Quaternion rot = _charManager.transform.rotation * Quaternion.Euler(0f, -90f, 0f);
		_currentProp = Instantiate(_objects[rand], pos, rot);

		_charManager.CallCoroutine(_coroutine);
	}

	public override void DeactivateEffects()
	{
		// particle effect
		GameManager.Instance.GeneratePropEffect();
		GameManager.Instance.DeactivatePropObject();

		_renderer.enabled = true;
		_currentProp.SetActive(false);
		_charManager.Camera.gameObject.SetActive(true);
		_charManager.EnableControllerMovements();

		_charManager.StopScriptableCoroutine(_coroutine);
	}

	private IEnumerator CheckMovement()
	{
		while (currentDuration >= 0.0f)
		{
			float mov = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("Vertical");

			if (mov != 0.0f) // moving
			{
				_hasToDeactivate = false;
				// the interface has to adapt to this sudden change too
				currentDuration = 0.001f;
				DeactivateEffects();
			}

			yield return null;
		}

		DeactivateEffects();
	}
}
