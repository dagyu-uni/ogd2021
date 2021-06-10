using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDynamic : MonoBehaviour
{
	public Transform endTrans = null;

	[HideInInspector] public IEnumerator _coroutine = null;

    private Transform _startPos = null;
	private float _interpolator = 0.0f;
	// used to know movement direction
	private bool toEnd = true;

	private void Start()
	{
		_startPos = transform;
		_coroutine = ActivateMovement();
	}

	public IEnumerator ActivateMovement()
	{
		while (true)
		{
			_interpolator += Time.deltaTime;
			if (_interpolator >= 1.0f)
			{
				_interpolator = 0.0f;
				toEnd = !toEnd;
			}

			if (toEnd)
			{
				transform.position = Vector3.Lerp(transform.position, endTrans.position, _interpolator);
			}
			else
			{

			}
			
			yield return null;
		}
	}
}
