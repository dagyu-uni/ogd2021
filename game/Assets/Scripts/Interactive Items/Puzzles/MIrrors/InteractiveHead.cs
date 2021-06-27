using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveHead : InteractiveItem
{
	[SerializeField] private string _infoText = null;
	[SerializeField] private AudioCollection _rotateHead = null;

	private Quaternion _startinRot;
	private float _interpolator = 0.0f;

	// used to prevent rotation bugs
	[HideInInspector] public int correctRot = 0;

	private void Awake()
	{
		// Start with a random orientation
		correctRot = Random.Range(0, 4) * 90;
		transform.rotation *= Quaternion.Euler(correctRot, 0.0f, 0.0f);
	}

	public override string GetText(CharacterManager cm)
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		if (_interpolator != 0.0f)
			return;

		_startinRot = transform.rotation;

		// Rotate head
		StartCoroutine(SlerpHead());
		correctRot = (correctRot + 90) % 360;
		// Play sound
		if (_rotateHead != null)
		{
			Debug.Log("SOUND");
			AudioManager.Instance.PlayOneShotSound(
				_rotateHead.MixerGroupName,
				_rotateHead.AudioClip.name,
				transform.position,
				_rotateHead.Volume,
				_rotateHead.SpatialBlend,
				_rotateHead.Priority
			);
		}
	}

	private IEnumerator SlerpHead()
	{
		Quaternion target = transform.rotation * Quaternion.Euler(90.0f, 0.0f, 0.0f);

		while (_interpolator <= 1.0f)
		{
			_interpolator += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(_startinRot, target, _interpolator);
			yield return null;
		}

		_interpolator = 0.0f;

	}
}
