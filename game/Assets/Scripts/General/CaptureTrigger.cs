using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CaptureTrigger : MonoBehaviour
{
	private PhotonView _photonView = null;

	private void Awake()
	{
		_photonView = GetComponent<PhotonView>();
	}

	private void OnTriggerExit(Collider other)
	{
		bool wiz1 = other.gameObject.CompareTag("Wizard_1");
		bool wiz2 = other.gameObject.CompareTag("Wizard_2");

		if (wiz1)
		{
			_photonView.RPC("CallSetFree", RpcTarget.All, Role.Wizard_1);
		}

		if (wiz2)
		{
			_photonView.RPC("CallSetFree", RpcTarget.All, Role.Wizard_2);
		}
	}

	[PunRPC]
	private void CallSetFree(Role role)
	{
		GameManager.Instance.SetFree(role);
	}
}
