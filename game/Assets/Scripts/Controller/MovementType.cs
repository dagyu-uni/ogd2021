using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementType : MonoBehaviour
{
	public Status changeTo;

	protected PlayerController player;
	protected PlayerMovement movement;
	protected PlayerInput playerInput;
	protected Status playerStatus;

	public virtual void Start()
	{
		player = GetComponent<PlayerController>();
		player.AddMovementType(this);
	}

	public virtual void SetPlayerComponents(PlayerMovement move, PlayerInput input)
	{
		movement = move;
		playerInput = input;
	}

	public virtual void Movement()
	{
		//Movement info
	}
}
