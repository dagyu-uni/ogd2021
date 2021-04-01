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
	[HideInInspector] public Status status;
	public LayerMask collisionLayer; //Default
	public float crouchHeight = 1f;
	public PlayerInfo info;

	[SerializeField] private float _maxStamina = 6f;
	[SerializeField] private float _staminaReserve = 4f;
	[SerializeField] private float _staminaLimit = 0f;
	[SerializeField] private float _staminaDepletion = 1f;
	[SerializeField] private float _staminaRecovery = 1f;
	[SerializeField] private HeadBob _headBob = new HeadBob();
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
	private bool _previouslyGrounded = true;   // used to know when landing
	private float _stamina;

	private List<MovementType> _movements = new List<MovementType>();

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

		info = new PlayerInfo(_movement.controller.radius, _movement.controller.height);
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
		//_headBob.RegisterEventCallback(1.5f, PlayFootStepSound, BocCallbackType.Vertical);
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
			_previouslyGrounded = false;
	}

	void UpdateMovingStatus()
	{
		// Change the recovery rate based on how much stamina has been used.
		if (_stamina < _secondStaminaThreshold)
			_staminaRecovery = 0.25f;
		else if (_stamina < _firstStaminaThreshold)
			_staminaRecovery = 0.75f;
		else
			_staminaRecovery = 1.0f;

		// Actual recovery and depletion of _stamina
		if (status == Status.sprinting && _stamina > 0)
			_stamina = Mathf.Max(0f, _stamina - Time.deltaTime * _staminaDepletion);
		else if (_stamina < _maxStamina)
			_stamina = Mathf.Min(_maxStamina, _stamina + Time.deltaTime * _staminaRecovery);

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
		HeadBob();
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

			_movement.Jump(Vector3.up, 1f);
			playerInput.ResetJump();
		}
	}

	private void HeadBob()
	{
		// we are not considering vertical speed for the head bob
		// (so that we dont have speed while crouching for example)
		Vector3 velocityXZ = new Vector3(_movement.controller.velocity.x, 0.0f, _movement.controller.velocity.z);
		// Are we moving
		if (velocityXZ.sqrMagnitude > 0.01f && _movement.grounded)
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

	// here for future uses based on interaction
	//public bool hasObjectInfront(float dis, LayerMask layer)
	//{
	//    Vector3 top = transform.position + (transform.forward * 0.25f);
	//    Vector3 bottom = top - (transform.up * info.halfheight);

	//    return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.forward, dis, layer).Length >= 1);
	//}
}

public class PlayerInfo
{
	public float rayDistance;
	public float radius;
	public float height;
	public float halfradius;
	public float halfheight;

	public PlayerInfo(float r, float h)
	{
		radius = r;
		height = h;
		halfradius = r * 0.5f;
		halfheight = h * 0.5f;
		rayDistance = halfheight + radius + 0.175f;
	}
}
