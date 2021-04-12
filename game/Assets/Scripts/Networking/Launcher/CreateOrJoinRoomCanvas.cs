using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class CreateOrJoinRoomCanvas : MonoBehaviour
	{
		[SerializeField] private CreateOrJoinRoomMenu _createOrJoinRoomMenu;
		[SerializeField] private RoomListingMenu _roomListingMenu;

		private NetworkingCanvasManager _networkingCanvasManager;

		public void FirstInitialize(NetworkingCanvasManager canvases)
		{
			_networkingCanvasManager = canvases;
			_createOrJoinRoomMenu.FirstInitialize(canvases);
			_roomListingMenu.FirstInitialize(canvases);
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
