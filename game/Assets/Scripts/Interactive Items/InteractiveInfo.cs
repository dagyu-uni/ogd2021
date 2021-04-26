using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interactive Item that simply shows some text in the HUD
public class InteractiveInfo : InteractiveItem
{
	// Inspector Assigned
	[TextArea(3, 10)]
	[SerializeField] private string _infoText = null;
	[SerializeField] private GameObject _interfaceReadOnly = null;

	public override string GetText()
	{
		return _infoText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		OpenCloseInterface(characterManager);
	}

	public void OpenCloseInterface(CharacterManager characterManager)
	{
		if (_interfaceReadOnly == null)
			return;

		if (_interfaceReadOnly.gameObject.activeInHierarchy)
		{
			_interfaceReadOnly.SetActive(false);
			characterManager.EnableControllerMovements();
			characterManager.EnableCameraMovements();
		}
		else
		{
			_interfaceReadOnly.SetActive(true);
			characterManager.DisableControllerMovements();
			characterManager.DisableCameraMovements();
		}
	}
}
