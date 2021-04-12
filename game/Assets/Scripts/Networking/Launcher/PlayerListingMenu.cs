using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class PlayerListingMenu : MonoBehaviourPunCallbacks
	{
		[SerializeField] private PlayerListing _playerListing;
		[SerializeField] private Transform _content;

		private List<PlayerListing> _listings = new List<PlayerListing>();
		private NetworkingCanvasManager _networkingCanvasManager;

		public override void OnEnable()
		{
			base.OnEnable();
			GetCurrentRoomPlayers();
		}

		public override void OnDisable()
		{
			base.OnDisable();
			for (int i = 0; i < _listings.Count; i++)
			{
				Destroy(_listings[i].gameObject);
			}
			_listings.Clear();
		}

		public void FirstInitialize(NetworkingCanvasManager canvases)
		{
			_networkingCanvasManager = canvases;
		}

		private void GetCurrentRoomPlayers()
		{
			foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
			{
				AddPlayerListing(playerInfo.Value);
			}
		}

		private void AddPlayerListing(Player player)
		{
			int index = _listings.FindIndex(x => x.Player == player);
			if (index != -1)
			{
				_listings[index].SetPlayerInfo(player);
			}
			else
			{
				PlayerListing listing = Instantiate(_playerListing, _content);
				if (listing != null)
				{
					listing.SetPlayerInfo(player);
					_listings.Add(listing);
				}
			}
		}

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			AddPlayerListing(newPlayer);
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			int index = _listings.FindIndex(x => x.Player == otherPlayer);
			if (index != -1)
			{
				Destroy(_listings[index].gameObject);
				_listings.RemoveAt(index);
			}
		}
	}
}
