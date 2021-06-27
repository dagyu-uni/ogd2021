using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	public class LauncherManager : MonoBehaviourPunCallbacks
	{
		[SerializeField] private GameObject menuManager;
		[SerializeField] private byte maxPlayersPerRoom = 3;
		[SerializeField] private string levelName;

		public GameObject connectionStatusPanel;
		public GameObject popUpMessagePanel;
		public GameObject leaveRoomButton;
		public GameObject loadRoomButton;

		private readonly string gameVersion = "1";
		private string popUpMessage;

		private void Awake()
		{
			PhotonNetwork.AutomaticallySyncScene = true;
		}

		private void Start()
		{
			PlayerPrefs.DeleteAll();
			connectionStatusPanel.SetActive(true);
			connectionStatusPanel.GetComponentInChildren<Text>().text = "Connecting to Photon...";

			if (!PhotonNetwork.IsConnected)
				ConnectToPhoton();
			else
			{
				connectionStatusPanel.GetComponentInChildren<Text>().text = "Connected to Photon!";
				connectionStatusPanel.GetComponentInChildren<Text>().color =
					Color.Lerp(Color.green, Color.white, 0.5f);
			}
		}

		private void ConnectToPhoton()
		{
			PhotonNetwork.GameVersion = gameVersion;
			PhotonNetwork.ConnectUsingSettings();
		}

		public override void OnConnected()
		{
			base.OnConnected();
			connectionStatusPanel.GetComponentInChildren<Text>().text = "Connected to Photon!";
			connectionStatusPanel.GetComponentInChildren<Text>().color =
				Color.Lerp(Color.green, Color.white, 0.5f);
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			Debug.LogError("Disconnected from Photon Network.\n" + cause.ToString());
			menuManager.SetActive(true);
			connectionStatusPanel.GetComponentInChildren<Text>().text = "Disconnected from Photon.";
			connectionStatusPanel.GetComponentInChildren<Text>().color =
				Color.Lerp(Color.red, Color.white, 0.5f);
		}

		public override void OnConnectedToMaster()
		{
			if (!PhotonNetwork.InLobby)
				PhotonNetwork.JoinLobby();
		}

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			popUpMessage = newPlayer.NickName + " joined the room";
			StartCoroutine(PopUpMessage(popUpMessage));
			if (PhotonNetwork.IsMasterClient)
			{
				if (PhotonNetwork.CurrentRoom.PlayerCount <= maxPlayersPerRoom) //<= per testare con 2
				{
					leaveRoomButton.SetActive(false);
					loadRoomButton.SetActive(true);
				}
			}
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			popUpMessage = otherPlayer.NickName + " left the room";
			StartCoroutine(PopUpMessage(popUpMessage));
			loadRoomButton.SetActive(false);
			leaveRoomButton.SetActive(true);
		}

		public void LoadRoom()
		{
			PhotonNetwork.LoadLevel(levelName);
		}

		IEnumerator PopUpMessage(string message)
		{
			popUpMessagePanel.GetComponentInChildren<Text>().text = message;
			popUpMessagePanel.SetActive(true);
			yield return new WaitForSeconds(3f);
			popUpMessagePanel.SetActive(false);
		}
	}
}
