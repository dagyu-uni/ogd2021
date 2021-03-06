using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public class CompassImage : MonoBehaviour
{
	// character transform
	public Transform followTarget;

	// changing cardinal positions at every run of the match.
	private float _offset = 0f;

	public float Offset { set { _offset = value; } }

	private void Start()
	{
		followTarget = NetworkingPlayerManager.LocalPlayerInstance.transform;
	}

	void LateUpdate()
	{
		if (followTarget != null)
		{
			transform.rotation = Quaternion.AngleAxis(followTarget.transform.rotation.eulerAngles.y + _offset, Vector3.forward);
		}
	}
}
