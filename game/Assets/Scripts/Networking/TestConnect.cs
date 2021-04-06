using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConnect : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
		Debug.Log("Connecting to server.");
		PhotonNetwork.GameVersion = "0.0.1";
		PhotonNetwork.ConnectUsingSettings();
    }

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to server.");
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("Disconnected from server for reason" + cause.ToString());
	}
}
