using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	public class CreateOrJoinRoomMenu : MonoBehaviourPunCallbacks
	{
		[Space(10)]
		[Header("Custom Variables")]

		[SerializeField] private byte maxPlayersPerRoom = 3;
		[SerializeField] private GameObject canvas;

		public InputField playerNameField;
		public InputField roomNameField;

		[Space(5)]
		[Header("Text Box")]

		public Text playerStatusLabel;

		private string playerName = "";
		private string roomName = "";
		private NetworkingCanvasManager _networkingCanvasManager;

		public void FirstInitialize(NetworkingCanvasManager canvases)
		{
			_networkingCanvasManager = canvases;
		}

		public void Start()
		{
			playerNameField.readOnly = false;
			roomNameField.readOnly = false;
		}

		public void SetPlayerName(string name)
		{
			playerName = name;
		}

		public void SetRoomName(string name)
		{
			roomName = name;
		}

		public void JoinRoom(string roomName)
		{
			roomNameField.text = roomName;
			this.roomName = roomName;
			JoinRoom();
		}

		public void JoinRoom()
		{
			if (playerName == null || playerName.Equals(""))
			{
				playerStatusLabel.text = "Username cannot be empty";
				return;
			}

			if (roomName == null || roomName.Equals(""))
			{
				playerStatusLabel.text = "Room Name cannot be empty";
				return;
			}

			if (PhotonNetwork.IsConnected)
			{
				PhotonNetwork.LocalPlayer.NickName = playerName;
				RoomOptions option = new RoomOptions();
				option.MaxPlayers = maxPlayersPerRoom;
				PhotonNetwork.JoinOrCreateRoom(roomName, option, TypedLobby.Default);
			}
		}

		public override void OnCreatedRoom()
		{
			base.OnCreatedRoom();
			if (PhotonNetwork.IsMasterClient)
			{
				playerStatusLabel.text = "Your are the lobby leader.\nWait for the other players to join.";
			}
			else
			{
				playerStatusLabel.text = "Connected to lobby.\nWait for the leader to start the game.";
			}
			_networkingCanvasManager.CurrentRoomCanvas.Show();
			_networkingCanvasManager.CreateOrJoinRoomCanvas.Hide();
		}

		public override void OnJoinRoomFailed(short returnCode, string message)
		{
			base.OnJoinRoomFailed(returnCode, message);
			playerStatusLabel.text = "Photon: " + message;
		}

		public override void OnJoinedRoom()
		{
			if (PhotonNetwork.IsMasterClient)
			{
				playerStatusLabel.text = "Your are the lobby leader.\nWait for the other players to join.";
			}
			else
			{
				playerStatusLabel.text = "Connected to lobby.\nWait for the leader to start the game.";
			}
			_networkingCanvasManager.CurrentRoomCanvas.Show();
			_networkingCanvasManager.CreateOrJoinRoomCanvas.Hide();
		}
	}
}
