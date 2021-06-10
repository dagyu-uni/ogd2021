using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// NOTE: the game manager is supposed to handle everything that requires
// a higher level management, usually aspects not directly related
// to the game objects themselves (e.g. for registering colliders IDs and so on). 

public class GameManager : MonoBehaviour
{
	// Singleton
	private GameManager() { }
	private static GameManager _instance = null;

	public static GameManager Instance { get { return _instance; } }

	private void Awake()
	{
		//Singleton
		if (_instance == null)
		{
			_instance = FindObjectOfType<GameManager>();

			if (_instance == null)
			{
				GameObject sceneManager = new GameObject("Game Manager");
				_instance = sceneManager.AddComponent<GameManager>();
			}
		}

		DontDestroyOnLoad(gameObject);

		_photonView = GetComponent<PhotonView>();
	}

	// Public
	[Tooltip("Expressed in seconds.")]
	[SerializeField] private float _remainingMatchTime = 75f;
	[SerializeField] private Transform _captureTransform = null;
	[Header("Particles")]
	[SerializeField] private List<ParticleSystem> _footprints = new List<ParticleSystem>();

	// Internals
	private bool _isGameOver = false;
	private Role winner;
	private int numOfTreasures = 0;
	private Coroutine _gameCycleRoutine = null;
	private PhotonView _photonView = null;

	// Every component in the scene has a unique id used as key of the dictionary
	private Dictionary<Role, PlayerInfo> _playersInfo = new Dictionary<Role, PlayerInfo>();
	private Dictionary<int, InteractiveItem> _interactiveItems = new Dictionary<int, InteractiveItem>();

	// Properties
	public Dictionary<Role, PlayerInfo> PlayersInfo { get { return _playersInfo; } }

	private void Start()
	{
		// game cycle
		if (_gameCycleRoutine == null)
		{
			_gameCycleRoutine = StartCoroutine(GameCycle());
		}
	}

	private void Update()
	{
		_remainingMatchTime -= Time.deltaTime;

		// check wizard captures
		int numOfCaptures = 0;

		foreach (PlayerInfo info in _playersInfo.Values)
		{
			// info.role > 0 means if it's a wizard, one or the other
			if (info.role > 0 && info.characterManager.IsCaptured)
			{
				numOfCaptures++;
			}
		}

		// check remeining time
		if (_remainingMatchTime <= 0.0f || numOfCaptures == 2)
		{
			_isGameOver = true;
			winner = Role.King;
		}
	}

	// Manage the whole match cycle
	private IEnumerator GameCycle()
	{
		// Play
		while (!_isGameOver)
		{
			// do stuff
			yield return null;
		}

		// Handle the game over and the winner
		if (winner == Role.King)
		{
			// do stuff
		}
		else
		{
			// do other stuff
		}
	}

	// called by players when they gather a treasure.
	public void AddTreasure()
	{
		numOfTreasures++;

		if (numOfTreasures >= 2)
		{
			// unlock doors or something
		}
	}

	// returns a properly formatted time string for the current match
	public string FormatMatchTime()
	{
		if (_remainingMatchTime < 0)
			return null;

		int minutes = Mathf.FloorToInt(_remainingMatchTime / 60.0f);
		int seconds = Mathf.FloorToInt(_remainingMatchTime % 60);

		string min = minutes >= 10 ? minutes.ToString() : "0" + minutes.ToString();
		string sec = seconds >= 10 ? seconds.ToString() : "0" + seconds.ToString();

		return min + " : " + sec;
	}

	// Process what should happen when the king captures a wizard.
	public void CaptureWizard(CharacterManager charManager)
	{
		if (charManager == null)
			return;

		// TODO play some particle effects (smoke or something)

		// teleport the wizard in a prison or something
		_photonView.RPC("TeleportWizard", RpcTarget.All, charManager.Role);
	}

	[PunRPC]
	public void TeleportWizard(Role role)
	{
		_playersInfo[role].collider.transform.position = _captureTransform.position;
		_playersInfo[role].collider.transform.rotation = _captureTransform.rotation;
	}

	// Register and Get a Player Info reference searched on by the instance ID of its collider
	public void RegisterPlayerInfo(Role role, PlayerInfo playerInfo)
	{
		if (!_playersInfo.ContainsKey(role))
		{
			_playersInfo[role] = playerInfo;
		}
	}

	public PlayerInfo GetPlayerInfo(Role role)
	{
		PlayerInfo playerInfo;
		if (_playersInfo.TryGetValue(role, out playerInfo))
		{
			return playerInfo;
		}

		return null;
	}

	// Register and Get a InteractiveItem reference searched on by the instance ID of its collider
	public void RegisterInteractiveItem(int colliderID, InteractiveItem interactiveItem)
	{
		if (!_interactiveItems.ContainsKey(colliderID))
		{
			_interactiveItems[colliderID] = interactiveItem;
		}
	}

	public InteractiveItem GetInteractiveItem(int colliderID)
	{
		InteractiveItem item;
		if (_interactiveItems.TryGetValue(colliderID, out item))
		{
			return item;
		}

		return null;
	}

	// helper function used to shuffle pseudo-randomly a list of strings(based on Fisher-Yates shuffle)
	public void Shuffle<T>(List<T> list)
	{
		System.Random rng = new System.Random();

		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	// Particles
	// footprints[0] = left, [1] = right
	public void GenerateFootprint(string tag, float height, bool leftOrRight)
	{
		// Note: 0.01744 = 1 / 57.3248
		// 57.3248 = 360 / 6.28

		_footprints[0].Stop();
		_footprints[1].Stop();

		Role role = tag == "Wizard_1" ? Role.Wizard_1 : Role.Wizard_2;
		Transform tr = _playersInfo[role].collider.transform;

		if (leftOrRight)    // left footprint
		{
			ParticleSystem.MainModule main_left = _footprints[0].main;
			main_left.startRotationY = tr.eulerAngles.y * 0.01744f;
			_footprints[0].transform.position = new Vector3(tr.position.x, height, tr.position.z);
			_footprints[0].Play();
		}
		else    // right footprint
		{
			ParticleSystem.MainModule main_right = _footprints[1].main;
			main_right.startRotationY = tr.eulerAngles.y * 0.01744f;
			_footprints[1].transform.position = new Vector3(tr.position.x, height, tr.position.z);
			_footprints[1].Play();
		}
	}
}

// Easily get all the player info you may need.
public class PlayerInfo
{
	public Collider collider;
	public CharacterManager characterManager;
	public Camera camera;
	public Role role;
}

[System.Serializable]
public enum Role { King, Wizard_1, Wizard_2 }
