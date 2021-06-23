using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorHandler : MonoBehaviour
{
	[SerializeField] private ReflectionProbe _probe = null;
	[SerializeField] private GameObject _mirrorHead = null;

	private int counter = 0;

	private void OnTriggerEnter(Collider other)
	{
		++counter;
		_probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("King"))
		{
			Vector3 kingHeadVec = (_mirrorHead.transform.position - other.transform.position).normalized;

			// only activate the head if the king is not looking in that direction
			if (Vector3.Dot(other.transform.forward, kingHeadVec) < 0.0f)
			{
				_mirrorHead.SetActive(true);
			}
			else
			{
				_mirrorHead.SetActive(false);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		--counter;

		// when there are no players in range, we don't want the probe to update at every frame
		if (counter == 0)
		{
			_probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
		}
	}

	// used by the mirrors manager to set mirrors and relative heads rotations
	public void SetRotation(int i)
	{
		Quaternion rot = Quaternion.Euler(i * -90.0f, 0.0f, 0.0f);
		_mirrorHead.transform.rotation *= rot;
	}
}
