using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
	[SerializeField] private List<Component> _randomizers = new List<Component>();
	[Tooltip("Expressed in seconds.")]
	[SerializeField] private float _remainingMatchTime = 75f;
	[SerializeField] private Transform _captureTransform = null;
	[SerializeField] private List<InteractiveDoor> _finalDoors = new List<InteractiveDoor>();
	[SerializeField] private PrisonTrigger _prisonTrigger = null;
	[Header("Particles")]
	[SerializeField] private List<ParticleSystem> _footprints = new List<ParticleSystem>();
	[SerializeField] private ParticleSystem _propParticle = null;
	[Header("Special Objects and Skills")]
	[SerializeField] private List<GameObject> _props = new List<GameObject>();
	[SerializeField] private Invisibility _invisibility = null;
	[SerializeField] private GameObject _eye = null;
	[SerializeField] private ParticleSystem _projectile = null;
	[SerializeField] private AudioCollection _captureAudio = null;


	// Internals
	private bool _isGameOver = false;
	private Role _winner;
	private string _winText;
	private string _loseText;
	private int numOfTreasures = 0;
	private Coroutine _gameCycleRoutine = null;
	private PhotonView _photonView = null;

	// Other Internals
	private GameObject _currentProp = null;
	private SkinnedMeshRenderer _kingRenderer = null;

	// Every component in the scene has a unique id used as key of the dictionary
	private Dictionary<Role, PlayerInfo> _playersInfo = new Dictionary<Role, PlayerInfo>();
	private Dictionary<int, InteractiveItem> _interactiveItems = new Dictionary<int, InteractiveItem>();

	// Properties
	public Dictionary<Role, PlayerInfo> PlayersInfo { get { return _playersInfo; } }

	private void Start()
	{

		if (_randomizers != null)
		{
			//INIT STATE RANDOM
			int seed = (int)PhotonNetwork.CurrentRoom.CustomProperties["createdAt"];
			Debug.Log("CURRENT SEED " + seed);
			_randomizers.ForEach(c => {
				Random.InitState(seed);
				c.GetComponent<Randomizer>().InitRandom();
			});
		}
		// game cycle
		if (_gameCycleRoutine == null)
		{
			_gameCycleRoutine = StartCoroutine(GameCycle());
		}

		_winText = "Victory!";
		_loseText = "Defeat!";
	}

	private void Update()
	{
		_remainingMatchTime -= Time.deltaTime;

		// check wizard captures
		//int numOfCaptures = 0;

		//foreach (PlayerInfo info in _playersInfo.Values)
		//{
		//	// info.role > 0 means if it's a wizard, one or the other
		//	if (info.role > 0 && info.characterManager.IsCaptured)
		//	{
		//		numOfCaptures++;
		//	}
		//}

		// check remaining time
		if (_remainingMatchTime <= 0.0f || _prisonTrigger.numOfWizard >= 2)
		{
			_photonView.RPC("GameOver", RpcTarget.All, Role.King);
		}
	}

	public void WizardEscaped()
	{
		_photonView.RPC("GameOver", RpcTarget.All, Role.Wizard_1);
	}

	[PunRPC]
	private void GameOver(Role winner)
	{
		_isGameOver = true;
		_winner = winner;
	}

	// Manage the whole match cycle
	private IEnumerator GameCycle()
	{
		// Play
		while (!_isGameOver)
		{
			yield return null;
		}

		// Handle the game over and the winner
		PlayerHUD kinghud = _playersInfo[Role.King].characterManager.PlayerHUD;
		PlayerHUD w1hud = _playersInfo[Role.Wizard_1].characterManager.PlayerHUD;
		PlayerHUD w2hud = _playersInfo[Role.Wizard_2].characterManager.PlayerHUD;

		if (_winner == Role.King)
		{
			// set correct text
			kinghud.Mission.color = kinghud.eventColors[1];
			kinghud.MissionText = _winText;

			w1hud.Mission.color = w1hud.eventColors[0];
			w1hud.MissionText = _loseText;

			w2hud.Mission.color = w2hud.eventColors[0];
			w2hud.MissionText = _loseText;
		}
		else
		{
			// set correct text
			kinghud.Mission.color = kinghud.eventColors[0];
			kinghud.MissionText = _loseText;

			w1hud.Mission.color = w1hud.eventColors[1];
			w1hud.MissionText = _winText;

			w2hud.Mission.color = w2hud.eventColors[1];
			w2hud.MissionText = _winText;
		}

		// Fade out
		foreach (var role in _playersInfo.Keys)
		{
			PlayerHUD hud = _playersInfo[role].characterManager.PlayerHUD;
			hud.Fade(hud._fadeTime, ScreenFadeType.FadeOut);
		}

		Invoke("ExitMatch", 5.0f);
	}

	private void ExitMatch()
	{
		SceneManager.LoadScene(0);
	}

	// called by players when they gather a treasure.
	public void AddTreasure()
	{
		_photonView.RPC("RegisterTreasure", RpcTarget.All);
	}

	[PunRPC]
	private void RegisterTreasure()
	{
		numOfTreasures++;

		if (numOfTreasures >= 2)
		{
			// unlock final doors
			for (int i = 0; i < _finalDoors.Count; i++)
			{
				_finalDoors[i].isFinalDoor = false;
			}
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
		Debug.Log("Capture");
		if (charManager == null)
			return;

		// play some particle effects (smoke or something)
		if (_captureAudio != null)
		{
			AudioManager.Instance.PlayOneShotSound(
				_captureAudio.MixerGroupName,
				_captureAudio.AudioClip.name,
				transform.position,
				_captureAudio.Volume,
				_captureAudio.SpatialBlend,
				_captureAudio.Priority
			);
		}

		// teleport the wizard in a prison or something
		_photonView.RPC("TeleportWizard", RpcTarget.All, charManager.Role);

		charManager.IsCaptured = true;
	}

	[PunRPC]
	public void TeleportWizard(Role role)
	{
		_playersInfo[role].collider.transform.position = _captureTransform.position;
		_playersInfo[role].collider.transform.rotation = _captureTransform.rotation;

		_playersInfo[role].characterManager.IsCaptured = true;
	}

	public void SetFree(Role role)
	{
		_playersInfo[role].characterManager.IsCaptured = false;
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

	////////////////////////////
	// Transformation Skill
	public void GeneratePropEffect()
	{
		_photonView.RPC("PropEffect", RpcTarget.All);
	}

	[PunRPC]
	private void PropEffect()
	{
		_propParticle.Stop();
		_propParticle.transform.position = _playersInfo[Role.King].characterManager.transform.position;
		_propParticle.Play();
	}

	public void GeneratePropObject(string objName, float height)
	{
		_photonView.RPC("SpawnObject", RpcTarget.Others, objName, height);
	}

	[PunRPC]
	private void SpawnObject(string objName, float height)
	{
		Transform king = _playersInfo[Role.King].characterManager.transform;
		_kingRenderer = _playersInfo[Role.King].renderers[0];
		_kingRenderer.enabled = false;
		Vector3 pos = king.position + Vector3.down * height;
		Quaternion rot = king.rotation * Quaternion.Euler(0f, -90f, 0f);
		for (int i = 0; i < _props.Count; i++)
		{
			if (_props[i].name == objName)
			{
				_currentProp = GameObject.Instantiate(_props[i], pos, rot);
			}
		}
	}

	public void DeactivatePropObject()
	{
		_photonView.RPC("DeactivateProp", RpcTarget.Others);
	}

	[PunRPC]
	private void DeactivateProp()
	{
		_currentProp.SetActive(false);
		_kingRenderer.enabled = true;
	}
	////////////////////////////

	////////////////////////////
	// Invisibility Skill
	public void Invisibility(Role role, bool activating)
	{
		_photonView.RPC("InvisibilityRPC", RpcTarget.Others, role, activating);
	}

	[PunRPC]
	private void InvisibilityRPC(Role role, bool activating)
	{
		for (int i = 0; i < _playersInfo[role].renderers.Length; i++)
		{
			if (activating)
			{
				_playersInfo[role].renderers[i].material = _invisibility.invisibleMaterial;
			}
			else
			{
				_playersInfo[role].renderers[i].material = _playersInfo[role].originalMaterials[i];
			}
		}
	}

	////////////////////////////


	////////////////////////////
	// King Eye Skill
	public void GenerateEye(Vector3 pos, Quaternion rot)
	{
		_photonView.RPC("SpawnEye", RpcTarget.All, pos, rot);
	}

	[PunRPC]
	private void SpawnEye(Vector3 pos, Quaternion rot)
	{
		_eye.transform.position = pos;
		_eye.transform.rotation = rot;

		_eye.SetActive(true);
		_projectile.gameObject.SetActive(false);
	}

	public void DeactivateEyeRPC()
	{
		_photonView.RPC("DeactivateEye", RpcTarget.All);
	}

	[PunRPC]
	private void DeactivateEye()
	{
		_eye.SetActive(false);
	}

	public void GenerateProjectile(float timeToCollide, Vector3 hitPoint)
	{
		_photonView.RPC("SpawnProjectile", RpcTarget.All, timeToCollide, hitPoint);
	}

	[PunRPC]
	private void SpawnProjectile(float timeToCollide, Vector3 hitPoint)
	{
		Transform kingTrans = _playersInfo[Role.King].characterManager.transform;
		_projectile.transform.position = _playersInfo[Role.King].characterManager.Camera.transform.position;
		_projectile.transform.rotation = _playersInfo[Role.King].characterManager.Camera.transform.rotation;
		ParticleSystem.MainModule main = _projectile.main;
		main.startLifetime = timeToCollide;

		_projectile.gameObject.SetActive(true);
	}


	////////////////////////////


	////////////////////////////
	// True Sight Skill
	public void GenerateSightFeedback(bool male, bool female)
	{
		_photonView.RPC("SightFeedback", RpcTarget.Others, male, female);
	}

	[PunRPC]
	private void SightFeedback(bool male, bool female)
	{
		if (male)
		{
			PlayerHUD hud = _playersInfo[Role.Wizard_1].characterManager.PlayerHUD;
			StartCoroutine(hud.SetEventText("The King can see you!", hud.eventColors[2]));
		}

		if (female)
		{
			PlayerHUD hud = _playersInfo[Role.Wizard_2].characterManager.PlayerHUD;
			StartCoroutine(hud.SetEventText("The King can see you!", hud.eventColors[2]));
		}
	}

	////////////////////////////
}

// Easily get all the player info you may need.
public class PlayerInfo
{
	public Collider collider;
	public CharacterManager characterManager;
	public SkinnedMeshRenderer[] renderers;
	public List<Material> originalMaterials = new List<Material>();
	public Camera camera;
	public Role role;
}

[System.Serializable]
public enum Role { King, Wizard_1, Wizard_2 }
