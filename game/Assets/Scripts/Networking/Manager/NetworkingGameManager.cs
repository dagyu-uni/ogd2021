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

		private GameObject player;

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
				InstanciatePlayerPrefab();

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

		private void InstanciatePlayerPrefab()
		{
			List<int> playerCodeList = new List<int>();
			int index=0, localPlayerCode = PhotonNetwork.LocalPlayer.ActorNumber;

			foreach (Player player in PhotonNetwork.PlayerList)
			{
				playerCodeList.Add(player.ActorNumber);
			}

			while (playerCodeList[index] != localPlayerCode)
				index++;

			if (index == 0)
				player = PhotonNetwork.Instantiate("King Container", KingSpawnPosition.transform.position,
						KingSpawnPosition.transform.rotation);
			else if (index == 1)
				player = PhotonNetwork.Instantiate("Wizard Container", Wizard1SpawnPosition.transform.position,
						Wizard1SpawnPosition.transform.rotation);
			else
				player = PhotonNetwork.Instantiate("Wizard Container", Wizard2SpawnPosition.transform.position,
						Wizard2SpawnPosition.transform.rotation);
		}
	}
}
