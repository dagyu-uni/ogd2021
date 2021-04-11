﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// collectable attributes
public class Collectable
{
	public int uiPriority;
	public string name;
	public GameObject gameObject;
	public PowerUp powerUp;
	public Sprite icon;
	[TextArea(3,10)]
	public string tooltipString;

	// the unique ID of who picked this collectable
	[HideInInspector] public int _collectorID = -1;
	// Rigidbody used to throw it away
	[HideInInspector] public Rigidbody rb = null;
}

public class CharacterManager : MonoBehaviour
{
    // Ispector Assigned
    [SerializeField] private Camera _camera = null;
    [SerializeField] private PlayerHUD _playerHUD = null;
    [SerializeField] private float _interactiveRayLength = 1.0f;
	[SerializeField] private AudioCollection _sprintingSounds = null;
	[SerializeField] private int _inventoryCapacity = 0;

	// Private
	private Collider _collider = null;
    private PlayerController _playerController = null;
    private CharacterController _characterController = null;
	private PlayerMovement _playerMovement = null;
	private CameraMovement _cameraMovement = null;
    private GameManager _gameManager = null;
    private AudioSource _audioSource = null;    // used for mouth sounds
	private AudioCollection _previousCollection = null;
	private int _previousClipPriority = 0;
    private int _interactiveMask = 0;

	// used to store which and how many objects the player has picked up
	private List<Collectable> _inventory = new List<Collectable>();

	// Cache all the player current speeds
	private CurrentSpeeds _currentSpeeds = new CurrentSpeeds();

	// Properties
	public PlayerController Controller { get { return _playerController; } }
	public PlayerMovement Movement { get { return _playerMovement; } }
	public Camera Camera { get { return _camera; } }

	private void Awake()
    {
        _collider = GetComponent<Collider>();
        _playerController = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
		_playerMovement = GetComponent<PlayerMovement>();
		_cameraMovement = _camera.GetComponent<CameraMovement>();
		_audioSource = GetComponent<AudioSource>();

        _interactiveMask = 1 << LayerMask.NameToLayer("Interactive");
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;

        if (_gameManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.collider = _collider;
            info.camera = _camera;
            info.characterManager = this;
            info.role = gameObject.tag == Role.King.ToString() ? Role.King : Role.Wizard;

            _gameManager.RegisterPlayerInfo(_collider.GetInstanceID(), info);
        }

        // When players spawn start fading in
        if (_playerHUD) _playerHUD.Fade(2.0f, ScreenFadeType.FadeIn);

		// Set inventory capacity
		_inventory.Capacity = _inventoryCapacity;
		_playerHUD.CapacityText = _inventory.Count.ToString() + " / " + _inventoryCapacity.ToString();
	}

    private void Update()
    {
		// Get Inventory Input
		if (Input.GetButtonDown("Inventory"))
		{
			if (_playerHUD.InventoryUI.activeInHierarchy)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
				_playerHUD.InventoryUI.SetActive(false);
				_playerHUD.InventoryTooltip.gameObject.SetActive(false);
				_playerHUD.InventoryIcon.color = new Color32(255, 255, 255, 255);
				EnableCameraMovements();
			}
			else
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				_playerHUD.InventoryUI.SetActive(true);
				_playerHUD.InventoryIcon.color = new Color32(170, 170, 170, 255);
				DisableCameraMovements();
			}
		}

		DetectInteractiveItems();

        // refresh HUD
        if (_playerHUD) _playerHUD.RefreshHUD(this);

		// Play Sprinting Sounds
		SprintingSounds();
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
                InteractiveItem item = _gameManager.GetInteractiveItem(hit.collider.GetInstanceID());

                if (item != null && item.RayPriority > highestPriority)
                {
                    priorityObject = item;
                    highestPriority = item.RayPriority;
                }
            }

            if (priorityObject != null)
            {
                if (_playerHUD)
                    _playerHUD.SetInteractionText(priorityObject.GetText());

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

		_inventory.Add(collectable);
		RefreshCollectablesHUD(collectable, true);
		if (collectable.powerUp != null)
			collectable.powerUp.ApplyPowerUp(this);

		// Cache the unique ID
		collectable._collectorID = _collider.GetInstanceID();

		return true;
	}

	// used when "using" a collectable
	public Collectable SubtractCollectable(string name)
	{
		Collectable res = null;
		for (int i = 0; i < _inventory.Count; i++)
		{
			if (_inventory[i].name == name)
			{
				res = _inventory[i];
				_inventory.RemoveAt(i);
				RefreshCollectablesHUD(res, false);
				if (res.powerUp != null)
					res.powerUp.RemovePowerUp(this);
				break;
			}
		}

		// Release the ID
		res._collectorID = -1;

		return res;
	}

	private void RefreshCollectablesHUD(Collectable collectable, bool added)
	{
		// Sort the inventory by descending order (=> higher priority will
		// be on the left in the inventory).
		_inventory.Sort((a, b) => b.uiPriority.CompareTo(a.uiPriority));

		_playerHUD.RefreshCollectables(_inventory, collectable, added);
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

	// Block all the player's movement, both controller and camera.
	public void DisableControllerMovements()
	{
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
		// Enable
		_playerMovement.walkSpeed = _currentSpeeds.walkSpeed;
		_playerMovement.runSpeed = _currentSpeeds.runSpeed;
		_playerMovement.crouchSpeed = _currentSpeeds.crouchSpeed;
		_playerMovement.JumpSpeed = _currentSpeeds.jumpSpeed;

		_playerController._hasInterfaceOpen = false;
	}

	public void DisableCameraMovements()
	{
		_currentSpeeds.SetCameraSensitivity(_cameraMovement.Sensitivity);
		_cameraMovement.Sensitivity = Vector2.zero;
	}

	public void EnableCameraMovements()
	{
		_cameraMovement.Sensitivity = _currentSpeeds.sensitivity;
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

		public void SetCameraSensitivity(Vector2 sens)
		{
			sensitivity = sens;
		}
	}
}
