using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used for wood footsteps sounds

public class FootstepsTrigger : MonoBehaviour
{
	
	[SerializeField] private AudioCollection _woodFootsteps = null;
	[SerializeField] private AudioCollection _rockFootsteps = null;

	private void OnTriggerEnter(Collider other)
	{
		PlayerController controller = other.gameObject.GetComponent<PlayerController>();

		if (controller != null)
		{
			controller.Footsteps = _woodFootsteps;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerController controller = other.gameObject.GetComponent<PlayerController>();

		if (controller != null)
		{
			controller.Footsteps = _rockFootsteps;
		}
	}
}
