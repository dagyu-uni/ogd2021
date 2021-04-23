using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun.Demo.PunBasics
{
	public class NetworkingGameManager : MonoBehaviourPunCallbacks
	{
		public static NetworkingGameManager Instance;

		public GameObject KingSpawnPosition;
		public GameObject Wizard1SpawnPosition;
		public GameObject Wizard2SpawnPosition;
		public List<GameObject> playerPrefabs;

		private void Start()
		{
			Instance = this;

			var pool = PhotonNetwork.PrefabPool as DefaultPool;
			if (pool != null && pool.ResourceCache.Count == 0 && playerPrefabs != null)
				foreach (var prefab in playerPrefabs)
					pool.ResourceCache.Add(prefab.name, prefab);

			if (!PhotonNetwork.IsConnected)
			{
				SceneManager.LoadScene("Launcher");
				return;
			}

			if (NetworkingPlayerManager.LocalPlayerInstance == null)
			{
				GameObject player;

				if (PhotonNetwork.IsMasterClient)
				{
					player = PhotonNetwork.Instantiate("King Container", KingSpawnPosition.transform.position,
						KingSpawnPosition.transform.rotation);
					//AudioManager.instance._ownerMapName = "FemaleMap";
				}
				else
				{
					player = PhotonNetwork.Instantiate("Wizard Container_1", Wizard1SpawnPosition.transform.position,
						Wizard1SpawnPosition.transform.rotation);
					//AudioManager.instance._ownerMapName = "MaleMap";
				}

				AudioManager.Instance.ListenerPos = player.GetComponentInChildren<AudioListener>().transform;
			}
		}

		public override void OnPlayerLeftRoom(Player other)
		{
			Debug.Log(other.NickName + "disconnected."); //Aggiugere eventualmente un pop up di avviso.
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Launcher");
		}

		public void ExitRoom()
		{
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Launcher");
		}
	}
}
