using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonTrigger : MonoBehaviour
{
	[HideInInspector] public int numOfWizard = 0;

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Wizard_1") || other.gameObject.CompareTag("Wizard_2"))
		{
			numOfWizard++;
		}
	}
}
