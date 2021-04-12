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

	// Every component in the scene has a unique id used as key of the dictionary
	private Dictionary<int, PlayerInfo> _playersInfo = new Dictionary<int, PlayerInfo>();
	private Dictionary<int, InteractiveItem> _interactiveItems = new Dictionary<int, InteractiveItem>();

	// Properties
	public Dictionary<int, PlayerInfo> PlayersInfo { get { return _playersInfo; } }

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
