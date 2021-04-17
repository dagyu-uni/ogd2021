using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	public class LeaveOrLoadRoomMenu : MonoBehaviour
	{
		[SerializeField] private byte maxPlayersPerRoom = 3;

		public Text roomCapacity;
		public Text playerStatusLabel;

		private NetworkingCanvasManager _networkingCanvasManager;

		private void Update()
		{
			PlayerStatusCheck();
			roomCapacity.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + maxPlayersPerRoom;
		}

		public void FirstInitialize(NetworkingCanvasManager canvases)
		{
			_networkingCanvasManager = canvases;
		}

		public void LoadRoom()
		{
			PhotonNetwork.LoadLevel("Prototype");
		}

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom(true);
			playerStatusLabel.enabled = false;
			_networkingCanvasManager.CurrentRoomCanvas.Hide();
			_networkingCanvasManager.CreateOrJoinRoomCanvas.Show();
		}

		private void PlayerStatusCheck()
		{
			if (PhotonNetwork.IsMasterClient)
			{
				if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
					playerStatusLabel.text = "Your are the lobby leader.\nPress Load Room to start the game.";
				else
					playerStatusLabel.text = "Your are the lobby leader.\nWait for other players to join.";
			}
			else
			{
				if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
					playerStatusLabel.text = "Connected to lobby.\nWait for the leader to start the game.";
				else
					playerStatusLabel.text = "Connected to lobby.\nWait for other players to join.";
			}
		}
	}
}
