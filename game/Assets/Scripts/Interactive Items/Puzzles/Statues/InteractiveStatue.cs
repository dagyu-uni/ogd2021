using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveStatue : InteractiveItem
{
	// consider where the statue is actually looking with the head
	public int offset = 0;

	[SerializeField] private string _infoText = null;

	private Quaternion _startinRot;
	private float _interpolator = 0.0f;

	private void Awake()
	{
		// Start with a random orientation
		float orientation = Random.Range(0, 8) * 45f;
		transform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + orientation, Vector3.up);
	}

	public override string GetText()
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		if (_interpolator != 0.0f)
			return;

		_startinRot = transform.rotation;

		// Rotate statue
		StartCoroutine(SlerpStatue());

		// TODO Play sound
	}

	private IEnumerator SlerpStatue()
	{
		Quaternion target = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + 45.0f, Vector3.up);

		while (_interpolator < 1.0f)
		{
			_interpolator += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(_startinRot, target, _interpolator);
			yield return null;
		}

		_interpolator = 0.0f;

	}
}
