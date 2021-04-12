using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class CurrentRoomCanvas : MonoBehaviour
	{
		[SerializeField] private LeaveOrLoadRoomMenu _leaveOrLoadRoomMenu;
		[SerializeField] private PlayerListingMenu _playerListingMenu;

		private NetworkingCanvasManager _networkingCanvasManager;

		public void FirstInitialize(NetworkingCanvasManager canvases)
		{
			_networkingCanvasManager = canvases;
			_playerListingMenu.FirstInitialize(canvases);
			_leaveOrLoadRoomMenu.FirstInitialize(canvases);
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}
