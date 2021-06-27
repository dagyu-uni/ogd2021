using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interactive Item that simply shows some text in the HUD
public class InteractiveInfo : InteractiveItem
{
	public List<Role> roles = new List<Role>(new Role[] { Role.King, Role.Wizard_1, Role.Wizard_2 });

	// Inspector Assigned
	[TextArea(3, 10)]
	[SerializeField] private string _infoText = null;
	[SerializeField] private GameObject _interfaceReadOnly = null;
	[SerializeField] private AudioCollection _audioCollection = null;

	public override string GetText(CharacterManager cm)
	{
		if (roles.Contains(cm.Role))
		{
			if(_audioCollection != null)
			{
				AudioManager.Instance.PlayOneShotSound(
					_audioCollection.MixerGroupName,
					_audioCollection[1].name,
					transform.position,
					_audioCollection.Volume,
					_audioCollection.SpatialBlend,
					_audioCollection.Priority
				);
			}
			return _infoText;
		}

		return "";
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
