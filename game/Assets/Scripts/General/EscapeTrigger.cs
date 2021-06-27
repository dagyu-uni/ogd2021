using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Wizard_1") || other.gameObject.CompareTag("Wizard_2"))
		{
			GameManager.Instance.WizardEscaped();
		}
	}
}
