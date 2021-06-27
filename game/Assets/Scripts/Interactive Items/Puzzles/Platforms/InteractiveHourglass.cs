using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveHourglass : InteractiveItem
{
	public PlatformDynamic platform = null;
	public List<InteractiveHourglass> otherHourglasses = new List<InteractiveHourglass>();
	public ParticleSystem sand = null;

	// Inspector Assigned
	[TextArea(3, 10)]
	[SerializeField] private string _infoText = null;

	private bool _isTimeRunning = false;

	public bool IsTimeRunning
	{
		get { return _isTimeRunning; }
		set { _isTimeRunning = value; }
	}

	public override string GetText(CharacterManager cm)
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		gameObject.GetComponent<PhotonView>().RequestOwnership();
		platform.GetComponent<PhotonView>().RequestOwnership();

		// handle related platform
		if (_isTimeRunning)
		{
			platform.DeactivateMovement();
			sand.Stop();
		}
		else
		{
			platform.ActivateMovement();
			sand.Play();
		}

		_isTimeRunning = !_isTimeRunning;

		// rotate the other hourglasses too
		for (int i = 0; i < otherHourglasses.Count; i++)
		{
			if (otherHourglasses[i].IsTimeRunning)
			{
				otherHourglasses[i].platform.DeactivateMovement();
				otherHourglasses[i].sand.Stop();
				otherHourglasses[i].IsTimeRunning = !otherHourglasses[i].IsTimeRunning;
			}

		}
	}
}
