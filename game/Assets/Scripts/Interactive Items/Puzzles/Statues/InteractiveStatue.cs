using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveStatue : InteractiveItem
{
	// consider where the statue is actually looking with the head
	public float currentOrientation = 0;

	[SerializeField] private string _infoText = null;
	[SerializeField] private AudioCollection _rotationAudio = null;

	private Quaternion _startinRot;
	private float _interpolator = 0.0f;

	protected override void Start()
	{
		base.Start();
		float angle = currentOrientation * 45f;
		Quaternion target = Quaternion.identity * Quaternion.AngleAxis(angle, Vector3.up);
		transform.rotation = target;
	}

	public override string GetText(CharacterManager cm)
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		gameObject.GetComponent<PhotonView>().RequestOwnership();
		if (_interpolator != 0.0f)
			return;


		_startinRot = transform.rotation;

		// Rotate statue
		currentOrientation = (currentOrientation + 1) % 8;
		StartCoroutine(SlerpStatue());

		// Rotation
		if (_rotationAudio != null)
		{
			AudioManager.Instance.PlayOneShotSound(
				_rotationAudio.MixerGroupName,
				_rotationAudio.AudioClip.name,
				transform.position,
				_rotationAudio.Volume,
				_rotationAudio.SpatialBlend,
				_rotationAudio.Priority
			);
		}
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
