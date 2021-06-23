using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun.Demo.PunBasics
{
	public class NetworkingGameManager : MonoBehaviourPunCallbacks
	{
		public static NetworkingGameManager Instance;

		[Space(5)]

		[Header("Player Information")]
		public List<GameObject> playerPrefabs;
		public List<GameObject> playerPositions;

		[Space(5)]

		[Header("Lockpick Information")]
		[Space(10)]
		public List<GameObject> lockpickPrefabs;
		public List<GameObject> lockpickPositions;

		[Space(5)]

		[Header("Statue Puzzle Information")]
		[Space(10)]
		public List<GameObject> compassPrefabs;

		[Space(5)]

		[Header("Painting Puzzle Information")]
		[Space(10)]
		public List<GameObject> PaintingItemPrefabs;

		private GameObject player;
		private DefaultPool prefabPool = PhotonNetwork.PrefabPool as DefaultPool;

		private void Start()
		{
			if (Instance == null)
				Instance = this;

			if (!PhotonNetwork.IsConnected)
			{
				SceneManager.LoadScene("Launcher");
				return;
			}

			PrefabPooling(playerPrefabs);

			if (NetworkingPlayerManager.LocalPlayerInstance == null)
			{
				PlayerInstantiation();
				AudioManager.Instance.ListenerPos = player.GetComponentInChildren<AudioListener>().transform;
			}

			if (PhotonNetwork.IsMasterClient)
			{
				PrefabPooling(lockpickPrefabs);
				LockpickInstatiation();
				PrefabPooling(compassPrefabs);
				PrefabPooling(PaintingItemPrefabs);
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

		private void PrefabPooling(List<GameObject> prefabList)
		{
			if (prefabPool != null && prefabList != null)
				foreach (GameObject prefab in prefabList)
					prefabPool.ResourceCache.Add(prefab.name, prefab);
		}

		private void PlayerInstantiation()
		{
			List<int> playerCodeList = new List<int>();
			int index = 0, localPlayerCode = PhotonNetwork.LocalPlayer.ActorNumber;

			foreach (Player player in PhotonNetwork.PlayerList)
			{
				playerCodeList.Add(player.ActorNumber);
			}

			while (playerCodeList[index] != localPlayerCode)
				index++;

			if (index == 0)
				player = PhotonNetwork.Instantiate(playerPrefabs[0].name,
					playerPositions[0].transform.position, playerPositions[0].transform.rotation);
			else if (index == 1)
				player = PhotonNetwork.Instantiate(playerPrefabs[1].name,
					playerPositions[1].transform.position, playerPositions[1].transform.rotation);
			else
				player = PhotonNetwork.Instantiate(playerPrefabs[2].name,
					playerPositions[2].transform.position, playerPositions[2].transform.rotation);
		}

		private void LockpickInstatiation()
		{
			int randomPrefabIndex, index;
			for (index = 0; index < lockpickPositions.Count; index++)
			{
				randomPrefabIndex = Random.Range(0, lockpickPrefabs.Count);
				PhotonNetwork.Instantiate(lockpickPrefabs[randomPrefabIndex].name,
					lockpickPositions[index].transform.position, lockpickPositions[index].transform.rotation);
			}
		}
	}
}
