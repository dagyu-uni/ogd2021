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

	//// used to memorize parents of the transforms entering in the trigger
	//private Dictionary<int, Transform> _triggerCache = new Dictionary<int, Transform>();
	//private GameObject target;

	//private float dist;
	//private Vector3 distVersor;

	private void Start()
	{
		_startPos = transform.position;
		_coroutine = MovementCoroutine();

		//dist = Vector3.Distance(endTrans.position, _startPos);
		//distVersor = (endTrans.position - _startPos).normalized;
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
		//gameObject.GetComponent<PhotonView>().RequestOwnership();
		StartCoroutine(_coroutine);
	}

	public void DeactivateMovement()
	{
		StopCoroutine(_coroutine);
	}

	//// make the characters be moved together with the platform
	//private void OnTriggerEnter(Collider other)
	//{
	//	string tag = other.gameObject.tag;

	//	if (tag != "King" && tag != "Wizard_1" && tag != "Wiizard_2")
	//		return;

	//	target = other.gameObject;
	//	// we need to cache the parent of the parent of the character trigger
	//	//Transform parent = other.transform.parent;
	//	//_triggerCache[other.GetInstanceID()] = parent.parent;
	//	//parent.parent = transform;

	//}

	//private void OnTriggerStay(Collider other)
	//{
	//	target.transform.position -= distVersor * dist * speed * Time.deltaTime; 
	//}

	//private void OnTriggerExit(Collider other)
	//{
	//	string tag = other.gameObject.tag;

	//	if (tag != "King" && tag != "Wizard_1" && tag != "Wiizard_2")
	//		return;

	//	int id = other.GetInstanceID();
	//	other.transform.parent.parent = _triggerCache[id];
	//	_triggerCache.Remove(id);
	//}
}
