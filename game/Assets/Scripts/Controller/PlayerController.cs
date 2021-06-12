using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Status { idle, walking, crouching, sprinting }

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
	// Public
	[HideInInspector] public Status status;
	public LayerMask collisionLayer; //Default
	public ControllerInfo info;

	// when players have an interface open some behaviors should change
	// e.g. not being able to move while solving a puzzle interface
	// or having the menu interface opened (useful for sounds too).
	[HideInInspector] public bool _hasInterfaceOpen = false;

	// Inspector Assigned
	[SerializeField] private float _maxStamina = 6f;
	[SerializeField] private float _staminaReserve = 4f;
	[SerializeField] private float _staminaLimit = 0f;
	[SerializeField] private float _staminaDepletion = 1f;
	[SerializeField] public float StaminaRecovery = 1f;
	private float _currentStaminaRecovery = 1f;
	[SerializeField] private HeadBob _headBob = new HeadBob();
	// Handle Sounds
	[SerializeField] private AudioCollection _footSteps = null;
	[SerializeField] private float _crouchAttenuation = 0.5f;

	// Internals
	private Vector3 _cameraLocalPos = Vector3.zero;
	private float _cameraResetFactor = 0f;
	private float _firstStaminaThreshold;
	private float _secondStaminaThreshold;

	private new CameraMovement camera;
	private PlayerMovement _movement;
	private PlayerInput playerInput;
	private AnimateCameraLevel animateCamLevel;

	private bool _forceStaminaReserve = false;
	private float _crouchCamAdjust;
	private float _uncrouchingFactor = 0f;
	private float crouchHeight = 0f;
	private float _stamina;

	// used to know when landing
	private bool _previouslyGrounded = true;
	private float _timeInAir = 0f;
	private bool _isJumping = false;
	private float _jumpCheck = 0.3f;
	private bool _canJumpAgain = true;
	private bool _hasFell = false;

	private List<MovementType> _movements = new List<MovementType>();

	// Properties
	public float MaxStamina { get { return _maxStamina; } }
	public float Stamina { get { return _stamina; } }
	public bool IsJumping { get { return _isJumping; } }
	public bool HasFell { get { return _hasFell; } }
	public PlayerMovement PlayerMovement
	{
		get { return _movement; }
	}

	private bool _areFootprintActive = true;

	public bool AreFootprintActive {
		get { return _areFootprintActive; }
		set { _areFootprintActive = value; }
	}

	public void ChangeStatus(Status s)
	{
		if (status == s)
			return;
		status = s;
	}

	public void AddMovementType(MovementType move)
	{
		if (_movements == null)
			_movements = new List<MovementType>();
		move.SetPlayerComponents(_movement, playerInput);

		_movements.Add(move);
	}

	private void Awake()
	{
		playerInput = GetComponent<PlayerInput>();
		_movement = GetComponent<PlayerMovement>();
	}

	private void Start()
	{
		_movement.AddToReset(() => { status = Status.walking; });

		camera = GetComponentInChildren<CameraMovement>();

		if (GetComponentInChildren<AnimateCameraLevel>())
			animateCamLevel = GetComponentInChildren<AnimateCameraLevel>();

		info = new ControllerInfo(_movement.controller.radius, _movement.controller.height);
		crouchHeight = info.height * 0.5f;
		_crouchCamAdjust = (crouchHeight - info.height) * 0.5f;
		_stamina = _maxStamina;
		_firstStaminaThreshold = _maxStamina * 0.5f;
		_secondStaminaThreshold = _maxStamina * 0.2f;

		// head bob
		// this allows to add an offset for the bobHead without any 
		// floating point numbers inaccuracies over time
		_cameraLocalPos = camera.transform.localPosition;
		_headBob.Initialize();
		// Register the play footStep sounds function at 1.5 time of the headBob curve.
		_headBob.RegisterEventCallback(1.5f, PlayFootStepSound, BobCallbackType.Vertical);
	}

	/******************************* UPDATE ******************************/
	void Update()
	{
		//Updates
		UpdateMovingStatus();

		//Checks
		CheckCrouching();

		//Misc
		UpdateCamLevel();

		// used for landing
		if (!_movement.grounded)
		{
			_previouslyGrounded = false;
			_timeInAir += Time.deltaTime;
		}
		else // grounded
		{
			_previouslyGrounded = true;
			_timeInAir = 0f;
		}

		// this is a dirty way to be sure that you are really jumping
		if (_isJumping && _movement.grounded && _jumpCheck <= 0f)
		{
			_isJumping = false;
			_jumpCheck = 0.3f;
		}
		else if (_isJumping)
		{
			_jumpCheck -= Time.deltaTime;
		}
	}

	void UpdateMovingStatus()
	{
		// Change the recovery rate based on how much stamina has been used.
		if (_stamina < _secondStaminaThreshold)
			_currentStaminaRecovery = 0.25f * StaminaRecovery;
		else if (_stamina < _firstStaminaThreshold)
			_currentStaminaRecovery = 0.75f * StaminaRecovery;
		else
			_currentStaminaRecovery = 1.0f * StaminaRecovery;

		// Actual recovery and depletion of _stamina
		if (status == Status.sprinting && _stamina > 0)
			_stamina = Mathf.Max(0f, _stamina - Time.deltaTime * _staminaDepletion);
		else if (_stamina < _maxStamina)
			_stamina = Mathf.Min(_maxStamina, _stamina + Time.deltaTime * _currentStaminaRecovery);

		// Change status if needed
		if ((int)status <= 1 || isSprinting())
		{
			if (playerInput.input.magnitude > 0.02f)
				ChangeStatus((shouldSprint()) ? Status.sprinting : Status.walking);
			else
				ChangeStatus(Status.idle);
		}
	}

	// Stamina System
	// If stamina gets to zero, you have to wait until the Reserve is full 
	// to sprint again. The limit (default=0) can be used to realize a more
	// realistic system. If the player goes under the limit he cant sprint
	// until his stamina goes beyond that point again.
	public bool shouldSprint()
	{
		bool sprint = false;
		sprint = (playerInput.run && playerInput.input.y > 0);

		if (!isSprinting()) //If we want to sprint
		{
			if (_forceStaminaReserve && _stamina < _staminaReserve)
				return false;
			else if (!_forceStaminaReserve && _stamina < _staminaLimit)
				return false;
		}
		if (_stamina <= 0)
		{
			_forceStaminaReserve = true;
			return false;
		}

		if (sprint)
			_forceStaminaReserve = false;
		return sprint;
	}

	void UpdateCamLevel()
	{
		if (animateCamLevel == null)
			return;

		float level = 0f;
		if (status == Status.crouching)
			level = _crouchCamAdjust;
		animateCamLevel.UpdateLevel(level, _crouchCamAdjust);
	}
	/*********************************************************************/


	/******************************** MOVE *******************************/
	void FixedUpdate()
	{
		foreach (MovementType moveType in _movements)
		{
			if (status == moveType.changeTo)
			{
				moveType.Movement();
				return;
			}
		}

		DefaultMovement();

		// Handle HeadBob     
		InitHeadBob();

		// Landing
		if (!_previouslyGrounded && _movement.grounded &&
			_timeInAir > _movement.controller.stepOffset * 0.3f &&
			status != Status.crouching)
		{
			if (!_isJumping)
				_hasFell = true;
			StartCoroutine(WaitToJumpAgain());
			PlayFootStepSound(true);
		}
		else
			_hasFell = false;
	}

	// you don't want to be able to immediatly jump after a landing, otherwise
	// the animator won't follow properly the movement.
	private IEnumerator WaitToJumpAgain()
	{
		yield return new WaitForSeconds(0.1f);
		_canJumpAgain = true;
	}

	private void DefaultMovement()
	{
		if (isSprinting() && isCrouching())
			Uncrouch();

		_movement.Move(playerInput.input, isSprinting(), isCrouching());
		if (_movement.grounded && playerInput.Jump())
		{
			if (status == Status.crouching)
			{
				if (!Uncrouch())
					return;
			}

			if (!_canJumpAgain)
				return;

			_movement.Jump(Vector3.up, 1f);
			playerInput.ResetJump();

			// Play jump start sound
			if (!_hasInterfaceOpen)
				PlayFootStepSound(false);

			_isJumping = true;
			_canJumpAgain = false;
		}
	}

	private void InitHeadBob()
	{
		// we are not considering vertical speed for the head bob
		// (so that we dont have speed while crouching for example)
		Vector3 velocityXZ = new Vector3(_movement.controller.velocity.x, 0.0f, _movement.controller.velocity.z);
		// Are we moving (a high value like 0.9f is used to avoid small glitches when barely moving)
		if (velocityXZ.sqrMagnitude > 0.9f && _movement.grounded)
		{
			float speed;
			float[] multipliers;
			_cameraResetFactor = 0f;

			if (status == Status.walking)
			{
				speed = _headBob.walkingSpeed;
				multipliers = _headBob.walkingMultipliers;
			}
			else if (status == Status.sprinting)
			{
				speed = _headBob.sprintingSpeed;
				multipliers = _headBob.sprintingMultipliers;
			}
			else
			{
				speed = _headBob.crouchingSpeed;
				multipliers = _headBob.crouchingMultipliers;
			}
			camera.transform.localPosition = _cameraLocalPos + _headBob.GetVectorOffset(speed, multipliers);
		}
		else if (camera.transform.localPosition != _cameraLocalPos)
		{
			_cameraResetFactor += Time.deltaTime;
			camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, _cameraLocalPos, _cameraResetFactor * 5f);
		}
	}

	public bool isSprinting()
	{
		return (status == Status.sprinting && _movement.grounded);
	}

	//public bool isWalking()
	//{
	//    if (status == Status.walking || status == Status.crouching)
	//        return (_movement.controller.velocity.magnitude > 0f && _movement.grounded);
	//    else
	//        return false;
	//}

	public bool isCrouching()
	{
		return (status == Status.crouching);
	}

	void CheckCrouching()
	{
		if (_hasInterfaceOpen)
			return;

		// avoid popping when uncrouching
		if (_uncrouchingFactor < 1 && status != Status.crouching)
		{
			_uncrouchingFactor = Mathf.Min(1.0f, _uncrouchingFactor + Time.deltaTime * 10f);
			_movement.controller.height = Mathf.Lerp(crouchHeight, info.height, _uncrouchingFactor);
		}

		if (!_movement.grounded || (int)status > 2)
			return;

		if (playerInput.run)
		{
			Uncrouch();
			return;
		}

		if (playerInput.crouch)
		{
			if (status != Status.crouching)
			{
				Crouch(true);
				_uncrouchingFactor = 0f;
			}
			else
				Uncrouch();
		}
	}

	public void Crouch(bool setStatus)
	{
		_movement.controller.height = crouchHeight;
		if (setStatus)
			ChangeStatus(Status.crouching);
	}

	public bool Uncrouch()
	{
		Vector3 bottom = transform.position - (Vector3.up * ((crouchHeight * 0.5f) - info.radius));
		bool isBlocked = Physics.SphereCast(bottom, info.radius, Vector3.up, out var hit, info.height - info.radius, collisionLayer);
		if (isBlocked)
			return false; //If we have something above us, do nothing and return
		ChangeStatus(Status.walking);
		return true;
	}
	/*********************************************************************/


	/******************************** SOUNDS *******************************/
	private void PlayFootStepSound()
	{
		if (AudioManager.Instance != null && _footSteps != null)
		{
			AudioClip clipToPlay;
			bool isCrouching = status == Status.crouching;
			if (status == Status.sprinting)
				clipToPlay = _footSteps[1];
			else
				clipToPlay = _footSteps[0];

			AudioManager.Instance.PhotonPlayOneShotSound(_footSteps.MixerGroupName, clipToPlay.name, transform.position,
													isCrouching ? _footSteps.Volume * _crouchAttenuation : _footSteps.Volume,
													_footSteps.SpatialBlend, _footSteps.Priority);
		}
	}

	private void PlayFootStepSound(bool landing)
	{
		if (AudioManager.Instance != null && _footSteps != null)
		{
			AudioClip clipToPlay;
			if (landing)
				clipToPlay = _footSteps[3];
			else // jumping
				clipToPlay = _footSteps[2];
			AudioManager.Instance.PhotonPlayOneShotSound(_footSteps.MixerGroupName, clipToPlay.name, transform.position,
													_footSteps.Volume, _footSteps.SpatialBlend, _footSteps.Priority);
		}
	}
	/*********************************************************************/


	// here for future uses based on interaction
	//public bool hasObjectInfront(float dis, LayerMask layer)
	//{
	//    Vector3 top = transform.position + (transform.forward * 0.25f);
	//    Vector3 bottom = top - (transform.up * info.halfheight);

	//    return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.forward, dis, layer).Length >= 1);
	//}
}

public class ControllerInfo
{
	public float rayDistance;
	public float radius;
	public float height;
	public float halfradius;
	public float halfheight;

	public ControllerInfo(float r, float h)
	{
		radius = r;
		height = h;
		halfradius = r * 0.5f;
		halfheight = h * 0.5f;
		rayDistance = halfheight + radius + 0.175f;
	}
}
