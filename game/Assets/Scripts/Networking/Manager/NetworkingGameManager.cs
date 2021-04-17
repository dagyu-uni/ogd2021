using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun.Demo.PunBasics
{
	public class NetworkingGameManager : MonoBehaviourPunCallbacks
	{
		public GameObject winnerUI;

		public GameObject player1SpawnPosition;
		public GameObject player2SpawnPosition;

		public List<GameObject> Prefabs;

		private GameObject player;

		// Start Method
		private void Start()
		{
			var pool = PhotonNetwork.PrefabPool as DefaultPool;
			if (pool != null && pool.ResourceCache.Count == 0 && Prefabs != null)
				foreach (var prefab in Prefabs)
					pool.ResourceCache.Add(prefab.name, prefab);

			if (!PhotonNetwork.IsConnected)
			{
				SceneManager.LoadScene("Launcher");
				return;
			}

			if (PlayerManager.LocalPlayerInstance == null)
			{
				if (PhotonNetwork.IsMasterClient)
				{
					Debug.Log("Instantiating Female");
					player = PhotonNetwork.Instantiate("OnlineFemalePrefab", player1SpawnPosition.transform.position,
						player1SpawnPosition.transform.rotation);
					GameObject.Find("MaleGrid").SetActive(false);
					//AudioManager.instance._ownerMapName = "FemaleMap";
				}
				else
				{
					Debug.Log("Instantiating Malemale");
					player = PhotonNetwork.Instantiate("OnlineMalePrefab", player2SpawnPosition.transform.position,
						player2SpawnPosition.transform.rotation);
					GameObject.Find("FemaleGrid").SetActive(false);
					//AudioManager.instance._ownerMapName = "MaleMap";
				}
			}
		}

		// Photon Methods
		public override void OnPlayerLeftRoom(Player other)
		{
			Debug.Log("OnPlayerLeftRoom() " + other.NickName); // See when others disconnect
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Launcher");
		}

		public void ExitRoom()
		{
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Launcher");
			SceneManager.LoadScene("Launcher");
		}
	}
}
