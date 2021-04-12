using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interactive Item that simply shows some text in the HUD
public class InteractiveInfo : InteractiveItem
{
	// Inspector Assigned
	[TextArea(3, 10)]
	[SerializeField] private string _infoText = null;

	public override string GetText()
	{
		return _infoText;
	}
}
