// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Photon.Pun.Demo.PunBasics
{
#pragma warning disable 649

	/// <summary>
	///     Player manager.
	///     Handles fire Input and Beams.
	/// </summary>
	public class NetworkingPlayerManager : MonoBehaviourPunCallbacks, IPunObservable
	{
		public static GameObject LocalPlayerInstance;

		[SerializeField] private GameObject _camera;

		private float Health = 1f;
		private GameObject beams;
		private bool IsFiring;

		#region MonoBehaviour CallBacks

		/// <summary>
		///     MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		public void Awake()
		{
			// #Important
			// used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
			if (photonView.IsMine)
			{
				LocalPlayerInstance = gameObject;
				_camera.SetActive(true);
			}
			else
			{
				GetComponent<PlayerInput>().enabled = false;
				GetComponent<PlayerMovement>().enabled = false;
				GetComponent<PlayerController>().enabled = false;
				GetComponent<CharacterManager>().enabled = false;
			}
			// #Critical
			// we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		///     MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		public void Start()
		{
#if UNITY_5_4_OR_NEWER
				// Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
				SceneManager.sceneLoaded += OnSceneLoaded;
#endif
		}

		public override void OnDisable()
		{
			// Always call the base to remove callbacks
			base.OnDisable();

#if UNITY_5_4_OR_NEWER
			SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
		}


		/// <summary>
		///     MonoBehaviour method called on GameObject by Unity on every frame.
		///     Process Inputs if local player.
		///     Show and hide the beams
		///     Watch for end of game, when local player health is 0.
		/// </summary>
		public void Update()
		{
			// we only process Inputs and check health if we are the local player
			if (photonView.IsMine)
			{
				ProcessInputs();

				if (Health <= 0f)
				{
					//GameManager.Instance.LeaveRoom();
				}
			}

			if (beams != null && IsFiring != beams.activeInHierarchy)
				beams.SetActive(IsFiring);
		}

		/// <summary>
		///     MonoBehaviour method called when the Collider 'other' enters the trigger.
		///     Affect Health of the Player if the collider is a beam
		///     Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
		///     One could move the collider further away to prevent this or check if the beam belongs to the player.
		/// </summary>
		public void OnTriggerEnter(Collider other)
		{
			if (!photonView.IsMine)
				return;


			// We are only interested in Beamers
			// we should be using tags but for the sake of distribution, let's simply check by name.
			if (!other.name.Contains("Beam"))
				return;

			Health -= 0.1f;
		}

		/// <summary>
		///     MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
		///     We're going to affect health while the beams are interesting the player
		/// </summary>
		/// <param name="other">Other.</param>
		public void OnTriggerStay(Collider other)
		{
			// we dont' do anything if we are not the local player.
			if (!photonView.IsMine)
				return;

			// We are only interested in Beamers
			// we should be using tags but for the sake of distribution, let's simply check by name.
			if (!other.name.Contains("Beam"))
				return;

			// we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
			Health -= 0.1f * Time.deltaTime;
		}


#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif


		/// <summary>
		///     MonoBehaviour method called after a new level of index 'level' was loaded.
		///     We recreate the Player UI because it was destroy when we switched level.
		///     Also reposition the player if outside the current arena.
		/// </summary>
		/// <param name="level">Level index loaded</param>
		private void CalledOnLevelWasLoaded(int level)
		{
			// check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
		}

		#endregion

		#region Private Methods

#if UNITY_5_4_OR_NEWER
		private void OnSceneLoaded(Scene scene, LoadSceneMode loadingMode)
		{
			CalledOnLevelWasLoaded(scene.buildIndex);
		}
#endif

		/// <summary>
		///     Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject
		///     (photonView.isMine == true)
		/// </summary>
		private void ProcessInputs()
		{
			if (Input.GetButtonDown("Fire1"))
			{
				// we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
				// notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
				if (EventSystem.current.IsPointerOverGameObject())
				{
					//	return;
				}

				if (!IsFiring)
					IsFiring = true;
			}

			if (Input.GetButtonUp("Fire1"))
				if (IsFiring)
					IsFiring = false;
		}

		#endregion

		#region IPunObservable implementation

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(IsFiring);
				stream.SendNext(Health);
			}
			else
			{
				// Network player, receive data
				IsFiring = (bool)stream.ReceiveNext();
				Health = (float)stream.ReceiveNext();
			}
		}

		#endregion
	}
}
