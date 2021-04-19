using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}

	// Public
	[Tooltip("Expressed in seconds.")]
	[SerializeField] private float _remainingMatchTime = 75f;
	[SerializeField] private Transform _captureTransform = null;

	// Internals
	private bool _isGameOver = false;

	// Every component in the scene has a unique id used as key of the dictionary
	private Dictionary<int, PlayerInfo> _playersInfo = new Dictionary<int, PlayerInfo>();
	private Dictionary<int, InteractiveItem> _interactiveItems = new Dictionary<int, InteractiveItem>();

	// Properties
	public Dictionary<int, PlayerInfo> PlayersInfo { get { return _playersInfo; } }

	private void Update()
	{
		_remainingMatchTime -= Time.deltaTime;

		if (_remainingMatchTime <= 0.0f)
			_isGameOver = true;

		//while (!_isGameOver)
		//{
		//	// do stuff
		//}
	}

	// returns a properly formatted time string for the current match
	public string FormatMatchTime()
	{
		if (_remainingMatchTime < 0) return null;

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
		charManager.transform.position = _captureTransform.position;
		charManager.transform.rotation = _captureTransform.rotation;
	}

	// Register and Get a Player Info reference searched on by the instance ID of its collider
	public void RegisterPlayerInfo(int key, PlayerInfo playerInfo)
	{
		if (!_playersInfo.ContainsKey(key))
		{
			_playersInfo[key] = playerInfo;
		}
	}

	public PlayerInfo GetPlayerInfo(int key)
	{
		PlayerInfo playerInfo;
		if (_playersInfo.TryGetValue(key, out playerInfo))
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
}

// Easily get all the player info you may need.
public class PlayerInfo
{
	public Collider collider;
	public CharacterManager characterManager;
	public Camera camera;
	public Role role;
}

public enum Role { King, Wizard }
