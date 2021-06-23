using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveHourglass : InteractiveItem
{
	public float rotationSpeed = 0.4f;
	public PlatformDynamic platform = null;
	public List<InteractiveHourglass> otherHourglasses = new List<InteractiveHourglass>();

	// Inspector Assigned
	[TextArea(3, 10)]
	[SerializeField] private string _infoText = null;

	private bool _isTimeRunning = false;
	private bool _isRotating = false;
	private float _interpolator = 0.0f;
	private Quaternion startRot;
	private Quaternion endRot;

	public bool IsTimeRunning
	{
		get { return _isTimeRunning; }
		set { _isTimeRunning = value; }
	}

	protected override void Start()
	{
		base.Start();
		startRot = transform.rotation;
		endRot = Quaternion.LookRotation(transform.forward, -transform.up);
	}

	public override string GetText(CharacterManager cm)
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		gameObject.GetComponent<PhotonView>().RequestOwnership();
		platform.GetComponent<PhotonView>().RequestOwnership();
		if (_isRotating)
			return;

		// handle related platform
		if (_isTimeRunning)
		{
			platform.DeactivateMovement();
		}
		else
		{
			platform.ActivateMovement();
		}

		_isTimeRunning = !_isTimeRunning;

		// handle hourglass
		StartCoroutine(RotateHourglass());
		// rotate the other hourglasses too
		for (int i = 0; i < otherHourglasses.Count; i++)
		{
			if (otherHourglasses[i].IsTimeRunning)
			{
				otherHourglasses[i].platform.DeactivateMovement();
				StartCoroutine(otherHourglasses[i].RotateHourglass());
				otherHourglasses[i].IsTimeRunning = !otherHourglasses[i].IsTimeRunning;
			}

		}
	}

	public IEnumerator RotateHourglass()
	{
		_isRotating = true;
		Quaternion fromRot = _isTimeRunning ? startRot : endRot;
		Quaternion toRot = _isTimeRunning ? endRot : startRot;

		while (_interpolator < 1.0f)
		{
			_interpolator = Mathf.Min(1.0f, _interpolator + Time.deltaTime * rotationSpeed);
			transform.rotation = Quaternion.Slerp(fromRot, toRot, _interpolator);
			yield return null;
		}

		_interpolator = 0.0f;
		_isRotating = false;
	}
}
