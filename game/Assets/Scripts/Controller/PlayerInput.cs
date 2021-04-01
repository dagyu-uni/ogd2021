using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gather all the player inputs.
public class PlayerInput : MonoBehaviour
{
	public Vector2 input
	{
		get
		{
			Vector2 i = Vector2.zero;
			i.x = Input.GetAxis("Horizontal");
			i.y = Input.GetAxis("Vertical");
			i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
			return i;
		}
	}

	public Vector2 raw
	{
		get
		{
			Vector2 i = Vector2.zero;
			i.x = Input.GetAxisRaw("Horizontal");
			i.y = Input.GetAxisRaw("Vertical");
			i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
			return i;
		}
	}

	public bool run
	{
		get { return Input.GetKey(KeyCode.LeftShift); }
	}

	public bool crouch
	{
		get { return Input.GetKeyDown(KeyCode.C); }
	}

	// used for staying crouched by keeping the button pressed
	public bool crouching
	{
		get { return Input.GetKey(KeyCode.C); }
	}

	//public float mouseScroll
	//{ 
	//    get { return Input.GetAxisRaw("Mouse ScrollWheel"); }
	//}


	private Vector2 _previous;
	private Vector2 _down;

	private int _jumpTimer;
	private bool _jump;

	void Start()
	{
		_jumpTimer = -1;
	}

	void Update()
	{
		_down = Vector2.zero;
		if (raw.x != _previous.x)
		{
			_previous.x = raw.x;
			if (_previous.x != 0)
				_down.x = _previous.x;
		}
		if (raw.y != _previous.y)
		{
			_previous.y = raw.y;
			if (_previous.y != 0)
				_down.y = _previous.y;
		}
	}

	public void FixedUpdate()
	{
		if (!Input.GetKey(KeyCode.Space))
		{
			_jump = false;
			_jumpTimer++;
		}
		else if (_jumpTimer > 0)
			_jump = true;
	}

	public bool Jump()
	{
		return _jump;
	}

	public void ResetJump()
	{
		_jumpTimer = -1;
	}
}
