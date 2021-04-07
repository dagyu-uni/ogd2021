using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Networking
{
	public class Launcher : MonoBehaviourPunCallbacks
	{
		[SerializeField] private GameObject controlPanel;
		[SerializeField] private byte maxPlayersPerRoom = 3;

		[Space(10)]
		[Header("Custom Variables")]
		[Space(5)]

		public Text connectionStatus;
		public Text playerStatus;	
		public GameObject buttonLoadRoom;

		private readonly string gameVersion = "1";

		private void Awake()
		{
			PhotonNetwork.AutomaticallySyncScene = true;
		}

		// Start Method
		private void Start()
		{
			PlayerPrefs.DeleteAll();
			connectionStatus.text = "Connecting to Photon!";
			Debug.Log("Connecting to Photon Network.");
			buttonLoadRoom.SetActive(false);

			if (!PhotonNetwork.IsConnected)
				ConnectToPhoton();
			else
			{
				connectionStatus.text = "Connected to Photon!";
				connectionStatus.color = Color.Lerp(Color.green, Color.white, 0.5f);
				buttonLoadRoom.SetActive(false);
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
			if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
				PhotonNetwork.LoadLevel("Basement"); //Start level for both players
			else
				playerStatus.text = "Exactly 3 players are required to start the game!";
		}

		// Photon Methods
		public override void OnConnected()
		{
			base.OnConnected();
			connectionStatus.text = "Connected to Photon!";
			connectionStatus.color = Color.Lerp(Color.green, Color.white, 0.5f);
			buttonLoadRoom.SetActive(false);
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			controlPanel.SetActive(true);
			connectionStatus.text = "Disconnected. Please check your Internet connection.";
			Debug.LogError("Disconnected from Photon Network.\n" + cause.ToString());
		}

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			playerStatus.text = newPlayer.NickName + " joined the room";
			buttonLoadRoom.SetActive(true);
		}

		public override void OnConnectedToMaster()
		{
			if (!PhotonNetwork.InLobby)
				PhotonNetwork.JoinLobby();
		}
	}
}
