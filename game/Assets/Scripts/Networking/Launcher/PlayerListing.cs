using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	public class PlayerListing : MonoBehaviour
	{
		[SerializeField] private Text _text;

		public Player Player { get; private set; }

		public void SetPlayerInfo(Player player)
		{
			Player = player;
			_text.text = player.NickName;
		}
	}
}
