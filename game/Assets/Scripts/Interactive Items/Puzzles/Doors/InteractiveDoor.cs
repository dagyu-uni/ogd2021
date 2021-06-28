using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveDoor : InteractiveItem
{
	public bool isLocked = false;
	public bool isFinalDoor = false

	[SerializeField] private GameObject _interface = null;
	[SerializeField] private InteractiveLever _lever = null;
	[SerializeField] public Door _door = null;
	[SerializeField] private float _closeAfterSeconds = 0;
	[Tooltip("Use the first bank for open door sounds and the second one for close door sounds.")]
	[SerializeField] private AudioCollection _audioCollection = null;

	private Quaternion _closedRot;
	private Quaternion _openedRot;
	private float _openInterp = 0.0f;
	private float _closeInterp = 0.0f;
	private CharacterManager charManager = null;
	private DoorPuzzle _doorPuzzle = null;

	private void Awake()
	{
		if (_interface != null)
		{
			_doorPuzzle = _interface.GetComponent<DoorPuzzle>();
		}
	}

	public override string GetText(CharacterManager cm)
	{
		if (isLocked)
			return "Open";
		else
			return "Close";
	}

	public override void Activate(CharacterManager cm)
	{
		charManager = cm;
		gameObject.GetComponent<PhotonView>().RequestOwnership();
		if (_door != null)
		{
			_door.Interaction(this, cm);
		}

	}

	public void StartPuzzle(CharacterManager cm)
	{
		if (_doorPuzzle != null)
		{
			// you need at least one lock pick
			// solve the door puzzle
			_doorPuzzle.ResetPuzzle();
			_interface.SetActive(true);
			cm.DisableControllerMovements();
			cm.DisableCameraMovements();
			cm.EnableCursor();
		}
	}


	private IEnumerator CloseAfterSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		CloseDoor();
	}

	public void TryOpenDoor(bool success)
	{
		if (success)
		{
			ToggleDoor(charManager);
			if (_lever != null)
				_lever.RestLever();
			if (_closeAfterSeconds > 0)
			{
				StartCoroutine(CloseAfterSeconds(_closeAfterSeconds));
			}
		}
		else
		{
			StartCoroutine(charManager.PlayerHUD.SetEventText("The lockpick broke!", charManager.PlayerHUD.eventColors[0]));
			charManager.SubtractCollectable(CollectableName.LockPick);
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

		GetComponent<PhotonView>().RPC("DoorToggled", RpcTarget.All);
		StartCoroutine(SlerpCloseDoor());
	}

	public void ToggleDoor(CharacterManager cm)
	{
		Debug.Log("LOCKED: " + isLocked);
		if (isLocked)
		{
			StartCoroutine(SlerpOpenDoor(GetDirection(cm)));
		}
		else
		{
			StartCoroutine(SlerpCloseDoor());
		}
		GetComponent<PhotonView>().RPC("DoorToggled", RpcTarget.All);
	}

	private IEnumerator SlerpOpenDoor(float directionSign)
	{
		Debug.Log("OPEN");
		Vector3 euler = new Vector3(0.0f, 90.0f * directionSign, 0.0f);
		_closedRot = transform.rotation;

		//  play door opening sound
		StartCoroutine(PlaySound(0));

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

		//play door close sound

		StartCoroutine(PlaySound(1));

		while (_closeInterp < 1.0f)
		{
			_closeInterp += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(_openedRot, Quaternion.Euler(euler), _closeInterp);
			yield return null;
		}

		_closeInterp = 0.0f;
	}

	private IEnumerator PlaySound(int bank)
	{
		if (_audioCollection != null)
		{
			AudioManager.Instance.PlayOneShotSound(
				_audioCollection.MixerGroupName,
				_audioCollection[bank].name,
				transform.position,
				_audioCollection.Volume,
				_audioCollection.SpatialBlend,
				_audioCollection.Priority
			);
		}
		yield break;
	}

	// returns 1 or -1 based on the toward direction of the player relative to the door
	private float GetDirection(CharacterManager cm)
	{
		return Mathf.Sign(Vector3.Dot(cm.transform.forward, transform.right));
	}

	[PunRPC]
	void DoorToggled()
	{
		isLocked = !isLocked;
	}

}
