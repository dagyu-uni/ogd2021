using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectableName { Passpartout, LockPick, SpeedIncrease, BypassJail, Ring, Dice, WornKey, GlobeMap, SkyrimMap, Quill, Compass, XRaySight, ThirdEye }
public enum ItemAction { Added, Throw, Used }

// collectable attributes
public class Collectable
{
	public int uiPriority;
	public CollectableName name;
	public bool isTreasure;
	public GameObject gameObject;
	public PowerUp powerUp;
	public Sprite icon;
	[TextArea(3, 10)]
	public string tooltipString;

	// the unique Role of who picked this collectable
	[HideInInspector] public Role role;
	// Rigidbody used to throw it away
	[HideInInspector] public Rigidbody rb = null;
	[HideInInspector] public GameObject collInterface = null;
}

public class CharacterManager : MonoBehaviour
{
	// Ispector Assigned
	[SerializeField] private Camera _camera = null;
	[SerializeField] private CameraMovement _bodyCameraMovement = null;
	[SerializeField] private PlayerHUD _playerHUD = null;
	[SerializeField] private float _interactiveRayLength = 1.0f;
	[SerializeField] private AudioCollection _sprintingSounds = null;
	// list of current skills owned by the player
	[SerializeField] private List<Skill> _skills = new List<Skill>();


	// Private
	private Role _role;
	private Collider _collider = null;
	private PlayerController _playerController = null;
	private CharacterController _characterController = null;
	private PlayerMovement _playerMovement = null;
	private CameraMovement _cameraMovement = null;
	private Animator _animator = null;
	private GameManager _gameManager = null;
	private AudioSource _audioSource = null;    // used for mouth sounds
	private AudioCollection _previousCollection = null;
	private int _previousClipPriority = 0;
	private int _interactiveMask = 0;
	// number of interfaces that disabled camera and/or movements
	private int _cameraDisabled = 0;
	private int _movementsDisabled = 0;
	private bool _previousNotLanding = true;
	private bool _isCaptured = false;

	// used to store which and how many objects the player has picked up
	private List<Collectable> _inventory = new List<Collectable>();

	// Cache all the player current speeds
	private CurrentSpeeds _currentSpeeds = new CurrentSpeeds();

	// Animator Hashes (for optimized calls)
	private int _speedXHash = Animator.StringToHash("SpeedX");
	private int _speedYHash = Animator.StringToHash("SpeedY");
	private int _crouchingHash = Animator.StringToHash("isCrouching");
	private int _walkingHash = Animator.StringToHash("isWalking");
	private int _sprintingHash = Animator.StringToHash("isSprinting");
	private int _jumpingHash = Animator.StringToHash("isJumping");
	private int _fellHash = Animator.StringToHash("hasFell");

	// Properties
	public Role Role { get { return _role; } }
	public Animator Animator { get { return _animator; } }
	public PlayerController Controller { get { return _playerController; } }
	public PlayerMovement Movement { get { return _playerMovement; } }
	public PlayerHUD PlayerHUD { get { return _playerHUD; } }
	public Camera Camera { get { return _camera; } }
	public List<Collectable> Inventory { get { return _inventory; } }
	public List<Skill> Skills { get { return _skills; } }
	public bool IsCaptured
	{
		get { return _isCaptured; }
		set { _isCaptured = value; }
	}

	private void Awake()
	{
		_collider = GetComponent<Collider>();
		_playerController = GetComponent<PlayerController>();
		_characterController = GetComponent<CharacterController>();
		_playerMovement = GetComponent<PlayerMovement>();
		_cameraMovement = _camera.GetComponent<CameraMovement>();
		_animator = GetComponent<Animator>();
		_audioSource = GetComponent<AudioSource>();

		_interactiveMask = 1 << LayerMask.NameToLayer("Interactive");

		_playerHUD.CharManager = this;
	}

	private void Start()
	{
		// Register this player info in the game manager
		_gameManager = GameManager.Instance;

		if (_gameManager != null)
		{
			PlayerInfo info = new PlayerInfo();
			info.collider = _collider;
			info.camera = _camera;
			info.characterManager = this;
			info.role = gameObject.tag == Role.King.ToString() ? Role.King :
						gameObject.tag == Role.Wizard_1.ToString() ? Role.Wizard_1 : Role.Wizard_2;
			_role = info.role;
			info.renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < info.renderers.Length; i++)
			{
				info.originalMaterials.Add(info.renderers[i].material);
			}

			_gameManager.RegisterPlayerInfo(_role, info);
		}

		// When players spawn start fading in
		if (_playerHUD)
			_playerHUD.Fade(_playerHUD._fadeTime, ScreenFadeType.FadeIn);

		// Inventory Capacity
		_inventory.Capacity = _playerHUD.InventoryCapacity;
		_playerHUD.CapacityText = _inventory.Count.ToString() + " / " + _inventory.Capacity.ToString();
		// Skills Capacity
		_skills.Capacity = _playerHUD.SkillsCapacity;

		// Initialize starting skills and set the structure in the HUD
		int maxP = 100;
		foreach (Skill skill in _skills)
		{
			skill.Initialize(this);
			//skill.isBaseSkill = true;
			skill.uiPriority = maxP;
			maxP -= 10;
		}

		DisableCursor();
	}

	private void Update()
	{
		UpdateAnimator();

		DetectInteractiveItems();

		// refresh HUD
		if (_playerHUD)
			_playerHUD.RefreshHUD(this);

		// Play Sprinting Sounds
		SprintingSounds();
	}

	private void UpdateAnimator()
	{
		_animator.SetFloat(_speedXHash, Input.GetAxis("Horizontal"));
		_animator.SetFloat(_speedYHash, Input.GetAxis("Vertical"));
		_animator.SetBool(_crouchingHash, _playerController.status == Status.crouching);
		_animator.SetBool(_walkingHash, _playerController.status == Status.walking);
		_animator.SetBool(_sprintingHash, _playerController.status == Status.sprinting);
		_animator.SetBool(_jumpingHash, _playerController.IsJumping);
		if (_playerController.HasFell && _previousNotLanding)
		{
			_animator.SetTrigger(_fellHash);
			_previousNotLanding = false;
			StartCoroutine(CanLandAgain());
		}
	}

	private IEnumerator CanLandAgain()
	{
		yield return new WaitForSeconds(0.1f);
		_previousNotLanding = true;
	}

	private void SprintingSounds()
	{
		if (!_playerMovement.grounded && _previousCollection == _sprintingSounds)
			StopSpokenSound();
		if (_playerController.status == Status.sprinting && !_playerController._hasInterfaceOpen)
		{
			SpeakSound(_sprintingSounds, 0, false);
		}
		else if (_playerController.status != Status.sprinting && _previousCollection == _sprintingSounds)
		{
			StopSpokenSound();
		}
	}

	private void DetectInteractiveItems()
	{
		// rays to handle interactive items
		Ray ray;
		RaycastHit hit;
		RaycastHit[] hits;

		// Process Interactive Objects
		ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
		// adjust ray length based on where we are looking (1 if ahead, more if in diagonal)
		float rayLength = Mathf.Lerp(_interactiveRayLength, _interactiveRayLength * 1.8f, Mathf.Abs(Vector3.Dot(_camera.transform.forward, Vector3.up)));
		// Cast the ray (only hitting the interactive layer)
		hits = Physics.RaycastAll(ray, rayLength, _interactiveMask);
		// Process hits
		if (hits.Length > 0)
		{
			// record which is the highest priority interactive item hit
			int highestPriority = int.MinValue;
			InteractiveItem priorityObject = null;

			for (int i = 0; i < hits.Length; i++)
			{
				hit = hits[i];
				InteractiveItem item = GameManager.Instance.GetInteractiveItem(hit.collider.GetInstanceID());

				if (item != null && item.RayPriority > highestPriority)
				{
					priorityObject = item;
					highestPriority = item.RayPriority;
				}
			}

			if (priorityObject != null)
			{
				if (_playerHUD)
					_playerHUD.SetInteractionText(priorityObject.GetText(this));

				if (Input.GetButtonDown("Use"))
					priorityObject.Activate(this);
			}
		}
		else    // no hits detected
		{
			if (_playerHUD)
				_playerHUD.SetInteractionText(null);
		}
	}

	// provide an API for collectable items
	public bool StoreCollectable(Collectable collectable)
	{
		// inventory full
		if (_inventory.Count == _inventory.Capacity)
		{
			StartCoroutine(_playerHUD.SetEventText("You can't take it. Inventory is full!", _playerHUD.eventColors[0]));
			return false;
		}

		// treasure
		if (collectable.isTreasure)
		{
			GameManager.Instance.AddTreasure();
		}


		// powerup
		if (collectable.powerUp != null && collectable.powerUp.HasSkill && _skills.Count == _skills.Capacity)
		{
			StartCoroutine(_playerHUD.SetEventText("You can't control any more skills for now.", _playerHUD.eventColors[0]));
			return false;
		}

		_inventory.Add(collectable);
		if (collectable.powerUp != null)
			collectable.powerUp.ApplyPowerUp(this);
		RefreshCollectablesHUD(collectable, ItemAction.Added);

		// Cache the unique role
		collectable.role = _role;

		return true;
	}

	// used when throwing a collectable
	public Collectable SubtractCollectable(CollectableName name)
	{
		Collectable res = null;
		for (int i = 0; i < _inventory.Count; i++)
		{
			if (_inventory[i].name == name)
			{
				res = _inventory[i];
				_inventory.RemoveAt(i);

				res.gameObject.GetComponent<InteractiveCollectable>().isPicked = false;

				if (res.powerUp != null)
					res.powerUp.RemovePowerUp(this);
				RefreshCollectablesHUD(res, ItemAction.Throw);
				break;
			}
		}

		return res;
	}

	public Collectable UseCollectable(CollectableName name)
	{
		Collectable res = null;
		for (int i = 0; i < _inventory.Count; i++)
		{
			if (_inventory[i].name == name)
			{
				res = _inventory[i];
				_inventory.RemoveAt(i);

				res.gameObject.GetComponent<InteractiveCollectable>().isPicked = false;

				if (res.powerUp != null)
					res.powerUp.RemovePowerUp(this);
				RefreshCollectablesHUD(res, ItemAction.Used);
				break;
			}
		}

		return res;
	}

	public bool HasCollectable(CollectableName name)
	{
		for (int i = 0; i < _inventory.Count; i++)
		{
			if (_inventory[i].name == name)
			{
				return true;
			}
		}

		return false;
	}

	public bool IsKing()
	{
		return this.Role == Role.King;
	}

	public bool IsWizard()
	{
		return this.Role == Role.Wizard_1 || this.Role == Role.Wizard_2;
	}


	private void RefreshCollectablesHUD(Collectable collectable, ItemAction action)
	{
		// Sort the inventory by descending order (=> higher priority will
		// be on the left in the inventory).
		_inventory.Sort((a, b) => b.uiPriority.CompareTo(a.uiPriority));

		_playerHUD.RefreshCollectables(_inventory, collectable, action);
	}

	// API to handle the audiosource (relative to the player's mouth sounds)
	// NOTE to work properly, every audio collection for the players' mouth sounds
	// should have different priority values.
	public void SpeakSound(AudioCollection voiceLine, int bank = 0, bool isLooping = false)
	{
		// if the new clip has a lower priority don't override the one you are playing
		if (_audioSource.clip == null || voiceLine.Priority > _previousClipPriority)
		{
			_previousCollection = voiceLine;
			_previousClipPriority = voiceLine.Priority;
			_audioSource.clip = voiceLine[bank];
			_audioSource.loop = isLooping;
			_audioSource.volume = voiceLine.Volume;
			_audioSource.spatialBlend = voiceLine.SpatialBlend;
			_audioSource.outputAudioMixerGroup = AudioManager.Instance.GetMixerGroupFromName(voiceLine.MixerGroupName);
			StartCoroutine(PlayCharacterSound(isLooping));
		}
	}

	public void StopSpokenSound()
	{
		_audioSource.Stop();
		_audioSource.clip = null;
	}

	private IEnumerator PlayCharacterSound(bool isLooping)
	{
		_audioSource.Play();
		if (!isLooping)
		{
			yield return new WaitForSeconds(_audioSource.clip.length);
			_audioSource.clip = null;
		}
	}

	// workaround to call a coroutine inside a scriptable object.
	public void CallCoroutine(IEnumerator c)
	{
		StartCoroutine(c);
	}

	public void StopScriptableCoroutine(IEnumerator c)
	{
		StopCoroutine(c);
	}

	// Disable/Enable all the player's movement, both controller and camera (and the cursor too).
	public void DisableControllerMovements()
	{
		_movementsDisabled--;
		if (_movementsDisabled != -1)
			return;

		// Save current values
		_currentSpeeds.SetControllerSpeeds(_playerMovement.walkSpeed, _playerMovement.runSpeed,
											_playerMovement.crouchSpeed, _playerMovement.JumpSpeed);
		// Disable
		_playerMovement.walkSpeed = 0f;
		_playerMovement.runSpeed = 0f;
		_playerMovement.crouchSpeed = 0f;
		_playerMovement.JumpSpeed = 0f;

		_playerController._hasInterfaceOpen = true;
	}

	public void EnableControllerMovements()
	{
		_movementsDisabled++;
		if (_movementsDisabled != 0)
			return;

		// Enable
		_playerMovement.walkSpeed = _currentSpeeds.walkSpeed;
		_playerMovement.runSpeed = _currentSpeeds.runSpeed;
		_playerMovement.crouchSpeed = _currentSpeeds.crouchSpeed;
		_playerMovement.JumpSpeed = _currentSpeeds.jumpSpeed;

		_playerController._hasInterfaceOpen = false;
	}

	public void DisableCameraMovements()
	{
		_cameraDisabled--;
		if (_cameraDisabled != -1)
			return;

		_currentSpeeds.SetCameraSensitivity(_cameraMovement.Sensitivity);
		_cameraMovement.Sensitivity = Vector2.zero;
		_bodyCameraMovement.Sensitivity = Vector2.zero;
	}

	public void EnableCameraMovements()
	{
		_cameraDisabled++;
		if (_cameraDisabled != 0)
			return;

		_cameraMovement.Sensitivity = _currentSpeeds.sensitivity;
		_bodyCameraMovement.Sensitivity = _currentSpeeds.sensitivity;
	}

	public void DisableCursor()
	{
		if (_cameraDisabled != 0)
			return;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public void EnableCursor()
	{
		if (_cameraDisabled != -1)
			return;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	private class CurrentSpeeds
	{
		public float walkSpeed;
		public float runSpeed;
		public float crouchSpeed;
		public float jumpSpeed;
		public Vector2 sensitivity;

		public void SetControllerSpeeds(float walk, float run, float crouch, float jump)
		{
			walkSpeed = walk;
			runSpeed = run;
			crouchSpeed = crouch;
			jumpSpeed = jump;
		}

		public void SetCameraSensitivity(Vector2 mainSens)
		{
			sensitivity = mainSens;
		}
	}

	// Set a procedural head movement based on player's main camera orientation
	// NOTE: remember to have the IK pass activated, otherwise it won't work.
	private void OnAnimatorIK(int layerIndex)
	{
		if (_animator != null)
		{
			_animator.SetLookAtWeight(0.4f);
			_animator.SetLookAtPosition(_camera.transform.position + _camera.transform.forward);
		}
	}

}
