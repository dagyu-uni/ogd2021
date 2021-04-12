using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	public class RoomListing : MonoBehaviour
	{
		[SerializeField] private Text _text;

		public RoomInfo RoomInfo { get; private set; }

		public void SetRoomInfo(RoomInfo roomInfo)
		{
			RoomInfo = roomInfo;
			_text.text = roomInfo.Name;
		}

		public void OnClick()
		{
			FindObjectOfType<CreateOrJoinRoomMenu>().JoinRoom(RoomInfo.Name);
		}
	}
}
