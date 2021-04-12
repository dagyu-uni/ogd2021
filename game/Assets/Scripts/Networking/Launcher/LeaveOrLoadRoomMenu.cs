using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class LeaveOrLoadRoomMenu : MonoBehaviour
	{
		private NetworkingCanvasManager _networkingCanvasManager;

		public void FirstInitialize(NetworkingCanvasManager canvases)
		{
			_networkingCanvasManager = canvases;
		}

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom(true);
			_networkingCanvasManager.CurrentRoomCanvas.Hide();
			_networkingCanvasManager.CreateOrJoinRoomCanvas.Show();
		}
	}
}
