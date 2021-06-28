using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonTrigger : MonoBehaviour
{
	[HideInInspector] public int numOfWizard = 0;

	private List<int> _ids = new List<int>();

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Wizard_1") || other.gameObject.CompareTag("Wizard_2"))
		{
			int id = other.gameObject.GetInstanceID();

			if (!_ids.Contains(id))
			{
				numOfWizard++;
				_ids.Add(id);
			}
		}
	}
}
