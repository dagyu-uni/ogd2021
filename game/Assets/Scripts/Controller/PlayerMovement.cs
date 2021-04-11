using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : InterpolatedTransform
{
	public float walkSpeed = 4.0f;
	public float runSpeed = 8.0f;
	public float crouchSpeed = 2f;
	[SerializeField] private float _jumpSpeed = 8.0f;
	[SerializeField] private float _gravity = 20.0f;
	[SerializeField] private float _antiBumpFactor = 0.75f;
	[HideInInspector] public Vector3 moveDirection = Vector3.zero;
	[HideInInspector] public CharacterController controller;
	[HideInInspector] public bool playerControl = false;
	[HideInInspector] public bool grounded = false;
	[HideInInspector] public Vector3 jump = Vector3.zero;

	private Vector3 _jumpedDir;
	private bool _forceGravity;
	private float _forceTime = 0;
	private float _jumpPower;
	private UnityEvent _onReset = new UnityEvent();

	// Properties
	public float JumpSpeed
	{
		get { return _jumpSpeed; }
		set { _jumpSpeed = value; }
	}

	public override void OnEnable()
	{
		base.OnEnable();
		controller = GetComponent<CharacterController>();
	}

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
	}

	public void AddToReset(UnityAction call)
	{
		_onReset.AddListener(call);
	}

	public override void ResetPositionTo(Vector3 resetTo)
	{
		controller.enabled = false;
		StartCoroutine(forcePosition());
		IEnumerator forcePosition()
		{
			//Reset position to 'resetTo'
			transform.position = resetTo;
			//Remove old interpolation
			ForgetPreviousTransforms();
			yield return new WaitForEndOfFrame();
		}
		controller.enabled = true;
		_onReset.Invoke();
	}

	public override void Update()
	{
		Vector3 newestTransform = m_lastPositions[m_newTransformIndex];
		Vector3 olderTransform = m_lastPositions[OldTransformIndex()];

		Vector3 adjust = Vector3.Lerp(olderTransform, newestTransform, InterpolationController.InterpolationFactor);
		adjust -= transform.position;

		controller.Move(adjust);

		if (_forceTime > 0)
			_forceTime -= Time.deltaTime;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_forceTime > 0)
		{
			if (_forceGravity)
				moveDirection.y -= _gravity * Time.deltaTime;
			grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
		}
	}

	public override void LateFixedUpdate()
	{
		base.LateFixedUpdate();
	}

	public void Move(Vector2 input, bool sprint, bool crouching)
	{
		if (_forceTime > 0)
			return;

		float speed = crouching ? crouchSpeed : (!sprint) ? walkSpeed : runSpeed;

		if (grounded)
		{
			moveDirection = new Vector3(input.x, -_antiBumpFactor, input.y);
			moveDirection = transform.TransformDirection(moveDirection) * speed;
			UpdateJump();
		}
		else
		{
			Vector3 adjust = new Vector3(input.x, 0, input.y);
			adjust = transform.TransformDirection(adjust);
			_jumpedDir += adjust * Time.fixedDeltaTime * _jumpPower * 2f;
			_jumpedDir = Vector3.ClampMagnitude(_jumpedDir, _jumpPower);
			moveDirection.x = _jumpedDir.x;
			moveDirection.z = _jumpedDir.z;
		}

		// Apply _gravity
		moveDirection.y -= _gravity * Time.deltaTime;
		// Move the controller, and set grounded true or false depending on whether we're standing on something
		grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
	}

	public void Jump(Vector3 dir, float mult)
	{
		jump = dir * mult;
	}

	public void UpdateJump()
	{
		if (jump != Vector3.zero)
		{
			Vector3 dir = (jump * _jumpSpeed);
			if (dir.x != 0)
				moveDirection.x = dir.x;
			if (dir.y != 0)
				moveDirection.y = dir.y;
			if (dir.z != 0)
				moveDirection.z = dir.z;

			Vector3 move = moveDirection;
			_jumpedDir = move;
			move.y = 0;
			_jumpPower = Mathf.Min(move.magnitude, _jumpSpeed);
			_jumpPower = Mathf.Max(_jumpPower, walkSpeed);
		}
		else
			_jumpedDir = Vector3.zero;
		jump = Vector3.zero;
	}
}
