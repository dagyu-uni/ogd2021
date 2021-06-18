using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDynamic : MonoBehaviour
{
	public float speed = 0.4f;
	public Transform endTrans = null;

	[HideInInspector] public IEnumerator _coroutine = null;

	private Vector3 _startPos;
	private float _interpolator = 0.0f;
	// used to know movement direction
	private bool toEnd = true;

	private void Start()
	{
		_startPos = transform.position;
		_coroutine = MovementCoroutine();
	}

	private IEnumerator MovementCoroutine()
	{
		while (true)
		{
			_interpolator = Mathf.Min(1.0f, _interpolator + Time.deltaTime * speed);
			if (_interpolator >= 1.0f)
			{
				_interpolator = 0.0f;
				toEnd = !toEnd;
			}

			if (toEnd)
			{
				transform.position = Vector3.Lerp(_startPos, endTrans.position, _interpolator);
			}
			else
			{
				transform.position = Vector3.Lerp(endTrans.position, _startPos, _interpolator);
			}

			yield return null;
		}
	}


	// API to activate and deactivate platform movement
	public void ActivateMovement()
	{
		gameObject.GetComponent<PhotonView>().RequestOwnership();
		StartCoroutine(_coroutine);
	}

	public void DeactivateMovement()
	{
		StopCoroutine(_coroutine);
	}
}
