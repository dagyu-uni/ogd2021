using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	public class Launcher : MonoBehaviourPunCallbacks
	{
		[SerializeField] private GameObject menuManager;
		[SerializeField] private byte maxPlayersPerRoom = 3;

		public Text connectionStatusLabel;
		public Text popUpMessageLabel;
		public GameObject leaveRoomButton;
		public GameObject loadRoomButton;

		private readonly string gameVersion = "1";
		private string popUpMessage;

		private void Awake()
		{
			PhotonNetwork.AutomaticallySyncScene = true;
		}

		// Start Method
		private void Start()
		{
			PlayerPrefs.DeleteAll();
			connectionStatusLabel.text = "Connecting to Photon!";

			if (!PhotonNetwork.IsConnected)
				ConnectToPhoton();
			else
			{
				connectionStatusLabel.text = "Connected to Photon!";
				connectionStatusLabel.color = Color.Lerp(Color.green, Color.white, 0.5f);
			}
		}

		// Tutorial Methods
		private void ConnectToPhoton()
		{
			PhotonNetwork.GameVersion = gameVersion;
			PhotonNetwork.ConnectUsingSettings();
		}

		//Onclick LoadRoomButton
		public void LoadRoom()
		{
			PhotonNetwork.LoadLevel("Prototype"); //Start level for both players
		}

		// Photon Methods
		public override void OnConnected()
		{
			base.OnConnected();
			connectionStatusLabel.text = "Connected to Photon!";
			connectionStatusLabel.color = Color.Lerp(Color.green, Color.white, 0.5f);
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			Debug.LogError("Disconnected from Photon Network.\n" + cause.ToString());
			menuManager.SetActive(true);
			connectionStatusLabel.text = "Disconnected. Please check your Internet connection.";
			connectionStatusLabel.color = Color.Lerp(Color.red, Color.white, 0.5f);
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

		public override void OnConnectedToMaster()
		{
			if (!PhotonNetwork.InLobby)
				PhotonNetwork.JoinLobby();
		}

		IEnumerator PopUpMessage(string Message)
		{
			popUpMessageLabel.text = Message;
			popUpMessageLabel.enabled = true;
			yield return new WaitForSeconds(3f);
			popUpMessageLabel.enabled = false;
		}
	}
}
