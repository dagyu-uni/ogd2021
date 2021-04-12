using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class RoomListingMenu : MonoBehaviourPunCallbacks
	{
		[SerializeField] private RoomListing _roomListing;
		[SerializeField] private Transform _content;

		private List<RoomListing> _listings = new List<RoomListing>();
		private NetworkingCanvasManager _networkingCanvasManager;

		public void FirstInitialize(NetworkingCanvasManager canvases)
		{
			_networkingCanvasManager = canvases;
		}

		private void Awake()
		{
			PhotonNetwork.JoinLobby();
		}

		public override void OnJoinedRoom()
		{
			_content.DetachChildren();
			_listings.Clear();
		}

		public override void OnRoomListUpdate(List<RoomInfo> roomList)
		{
			foreach (RoomInfo info in roomList)
			{
				//Removed to the room list
				if (info.RemovedFromList)
				{
					int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
					if (index != -1)
					{
						Destroy(_listings[index].gameObject);
						_listings.RemoveAt(index);
					}
				}
				//Added to the room list
				else
				{
					int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
					if (index == -1)
					{
						RoomListing listing = Instantiate(_roomListing, _content);
						if (listing != null)
						{
							listing.SetRoomInfo(info);
							_listings.Add(listing);
						}
					}
					else
					{
						//Modify listing
					}
				}
			}
		}
	}
}
