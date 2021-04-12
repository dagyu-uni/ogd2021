using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class NetworkingCanvasManager : MonoBehaviour
	{
		[SerializeField] private CreateOrJoinRoomCanvas _createOrJoinRoomCanvas;
		public CreateOrJoinRoomCanvas CreateOrJoinRoomCanvas
		{
			get
			{
				return _createOrJoinRoomCanvas;
			}
		}

		[SerializeField] private CurrentRoomCanvas _currentRoomCanvas;
		public CurrentRoomCanvas CurrentRoomCanvas
		{
			get
			{
				return _currentRoomCanvas;
			}
		}

		private void Awake()
		{
			FirstInitialize();
		}

		public void FirstInitialize()
		{
			CreateOrJoinRoomCanvas.FirstInitialize(this);
			CurrentRoomCanvas.FirstInitialize(this);
		}
	}
}
