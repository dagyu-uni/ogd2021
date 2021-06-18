using Photon.Pun;
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
		gameObject.GetComponent<PhotonView>().RequestOwnership();
		bool wizardCanOpen = (cm.IsWizard() && cm.HasCollectable(CollectableName.Passpartout) && !_isBend);
		if (_interpolator != 0.0f)
		{
			StartCoroutine(cm.PlayerHUD.SetEventText("Wait the lever action", cm.PlayerHUD.eventColors[0]));
		}
		else if (cm.IsKing() || wizardCanOpen)
		{
			// use it
			_door.ToggleDoor(cm);
			ToggleLever();
		}
		else
		{
			// wizards can't use it
			StartCoroutine(cm.PlayerHUD.SetEventText("The lever is too heavy!", cm.PlayerHUD.eventColors[0]));
		}
	}


	private void ToggleLever()
	{
		if (_isBend)
		{
			StartCoroutine(SlerpRestLever());
		}
		else
		{
			StartCoroutine(SlerpBendLever());
		}
		_isBend = !_isBend;
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
