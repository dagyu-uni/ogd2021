using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveDoor : InteractiveItem
{
	public bool isLocked = false;

	[SerializeField] private GameObject _interface = null;
	[SerializeField] private InteractiveLever _lever = null;

	private Quaternion _closedRot;
	private Quaternion _openedRot;
	private float _openInterp = 0.0f;
	private float _closeInterp = 0.0f;
	private CharacterManager charManager = null;
	private DoorPuzzle _doorPuzzle = null;

	private void Awake()
	{
		_doorPuzzle = _interface.GetComponent<DoorPuzzle>();
	}

	public override string GetText()
	{
		if (isLocked)
			return "Open";
		else
			return "Close";
	}

	public override void Activate(CharacterManager cm)
	{
		if (isLocked)
		{
			charManager = cm;
			// if you are the king you can always open a door
			if (cm.Role == Role.King && _openInterp == 0.0f)
			{
				// open door
				isLocked = false;
				_lever.RestLever();
				StartCoroutine(SlerpOpenDoor(GetDirection()));
			}
			// wizards
			else if (cm.Role != Role.King)
			{
				// you need at least one lock pick
				Collectable lockPick = cm.UseCollectable("LockPick");
				if (lockPick != null)
				{
					// solve the door puzzle
					_doorPuzzle.ResetPuzzle();
					_interface.SetActive(true);
					cm.DisableControllerMovements();
					cm.DisableCameraMovements();
					cm.EnableCursor();
				}
				else
				{
					// TODO play closed door sound
					StartCoroutine(cm.PlayerHUD.SetEventText("You need a lockpick to open the door", cm.PlayerHUD.eventColors[0]));
				}
			}
		}
		else
		{
			StartCoroutine(cm.PlayerHUD.SetEventText("The door is blocked", cm.PlayerHUD.eventColors[0]));
		}

	}

	public void TryOpenDoor(bool success)
	{
		if (!isLocked)
			return;

		if (success)
		{
			isLocked = false;
			_lever.RestLever();
			StartCoroutine(SlerpOpenDoor(GetDirection()));
		}
		else
		{
			StartCoroutine(charManager.PlayerHUD.SetEventText("The lockpick broke!", charManager.PlayerHUD.eventColors[0]));
		}

		charManager.EnableControllerMovements();
		charManager.EnableCameraMovements();
		charManager.DisableCursor();
	}

	// used only when the king uses the lever
	public void CloseDoor()
	{
		if (isLocked)
			return; 

		isLocked = true;
		StartCoroutine(SlerpCloseDoor());
	}

	private IEnumerator SlerpOpenDoor(float directionSign)
	{
		Vector3 euler = new Vector3(0.0f, 90.0f * directionSign, 0.0f);
		_closedRot = transform.rotation;

		// TODO play door opening sound

		while (_openInterp < 1.0f)
		{
			_openInterp += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(_closedRot, Quaternion.Euler(euler), _openInterp);
			yield return null;
		}

		_openInterp = 0.0f;
	}

	private IEnumerator SlerpCloseDoor()
	{
		Vector3 euler = new Vector3(0.0f, 0.0f, 0.0f);
		_openedRot = transform.rotation;

		// TODO play door close sound

		while (_closeInterp < 1.0f)
		{
			_closeInterp += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(_openedRot, Quaternion.Euler(euler), _closeInterp);
			yield return null;
		}

		_closeInterp = 0.0f;
	}

	// returns 1 or -1 based on the toward direction of the player relative to the door
	private float GetDirection()
	{
		return Mathf.Sign(Vector3.Dot(charManager.transform.forward, transform.right));
	}
}
