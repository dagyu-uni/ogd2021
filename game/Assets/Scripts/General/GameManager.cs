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
}
