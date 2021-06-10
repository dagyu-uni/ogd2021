using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveLever : InteractiveItem
{
	[SerializeField] private string _infoText = null;
	[SerializeField] private InteractiveDoor _door = null;

	// when the lever is bend the relative door is locked
	private bool _isBend = false;
	private Transform _restPose = null;
	private Transform _bendedPose = null;
	private float _interpolator = 0.0f;

	private void Awake()
	{
		_restPose = transform;
	}

	public override string GetText(CharacterManager cm)
	{
		return _infoText;
	}

	public override void Activate(CharacterManager cm)
	{
		if (cm.Role == Role.King)
		{
			// use it
			_door.CloseDoor();
			BendLever();
		}
		else
		{
			// wizards can't use it
			StartCoroutine(cm.PlayerHUD.SetEventText("The lever is too heavy!", cm.PlayerHUD.eventColors[0]));
		}
	}

	private void BendLever()
	{
		if (_isBend)
			return;

		_isBend = true;
		StartCoroutine(SlerpBendLever());
	}

	private IEnumerator SlerpBendLever()
	{
		Vector3 euler = new Vector3(45.0f, 0.0f, 0.0f);

		// TODO play lever use sound

		while (_interpolator < 1.0f)
		{
			_interpolator += Time.deltaTime * 0.5f;
			transform.rotation = Quaternion.Slerp(_restPose.rotation, Quaternion.Euler(euler), _interpolator);
			yield return null;
		}

		_bendedPose = transform;
		_interpolator = 0.0f;
	}

	public void RestLever()
	{
		if (!_isBend)
			return;

		_isBend = false;
		StartCoroutine(SlerpRestLever());
	}

	public IEnumerator SlerpRestLever()
	{
		Vector3 euler = new Vector3(-45.0f, 0.0f, 0.0f);

		// TODO play lever use sound

		while (_interpolator < 1.0f)
		{
			_interpolator += Time.deltaTime * 0.5f;
			transform.rotation = Quaternion.Slerp(_bendedPose.rotation, Quaternion.Euler(euler), _interpolator);
			yield return null;
		}

		_interpolator = 0.0f;
	}
}
