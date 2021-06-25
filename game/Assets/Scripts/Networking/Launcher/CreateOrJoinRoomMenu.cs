using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Photon.Pun.Demo.PunBasics
{
	public class CreateOrJoinRoomMenu : MonoBehaviourPunCallbacks
	{
		[SerializeField] private byte maxPlayersPerRoom = 3;
		[SerializeField] private GameObject canvas;

		public InputField playerNameField;
		public InputField roomNameField;
		public GameObject playerStatusPanel;

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
			if (playerName == null || playerName.Equals("") || roomName == null || roomName.Equals(""))
			{
				playerStatusPanel.SetActive(true);
				playerStatusPanel.GetComponentInChildren<Text>().text =
					"Username and room name cannot be empty";
				return;
			}

			if (PhotonNetwork.IsConnected)
			{
				PhotonNetwork.LocalPlayer.NickName = playerName;
				RoomOptions option = new RoomOptions();
				option.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
				option.CustomRoomProperties.Add("createdAt", (int)DateTime.Now.Ticks);
				option.MaxPlayers = maxPlayersPerRoom;
				PhotonNetwork.JoinOrCreateRoom(roomName, option, TypedLobby.Default);
			}
		}

		public override void OnCreatedRoom()
		{
			base.OnCreatedRoom();
			_networkingCanvasManager.CurrentRoomCanvas.Show();
			_networkingCanvasManager.CreateOrJoinRoomCanvas.Hide();
			playerStatusPanel.SetActive(true);
		}

		public override void OnJoinedRoom()
		{
			_networkingCanvasManager.CurrentRoomCanvas.Show();
			_networkingCanvasManager.CreateOrJoinRoomCanvas.Hide();
			playerStatusPanel.SetActive(true);
		}

		public override void OnJoinRoomFailed(short returnCode, string message)
		{
			base.OnJoinRoomFailed(returnCode, message);
			playerStatusPanel.SetActive(true);
			playerStatusPanel.GetComponentInChildren<Text>().text = "Photon: " + message;
		}
	}
}
