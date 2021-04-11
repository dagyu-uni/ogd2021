using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
	public GameObject characterBody;

	[SerializeField] private Vector2 _clampInDegrees = new Vector2(360, 180);
	[SerializeField] private Vector2 _sensitivity = new Vector2(2, 2);
	[SerializeField] private Vector2 _smoothing = new Vector2(3, 3);

	private Vector2 _mouseAbsolute;
	private Vector2 _smoothMouse;

	private Quaternion _targetOrientation;
	private Quaternion _targetCharacterOrientation;

	// Properties
	public Vector2 Sensitivity
	{
		get { return _sensitivity; }
		set { _sensitivity = value; }
	}

	void Start()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		// Cache camera's initial orientation.
		_targetOrientation = Quaternion.Euler(transform.localRotation.eulerAngles);

		// Cache character initial orientation for the target (player by default).
		if (characterBody)
			_targetCharacterOrientation = Quaternion.Euler(characterBody.transform.localRotation.eulerAngles);
	}

	void Update()
	{
		// Get raw mouse input for a cleaner reading on more sensitive mice.
		Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		// Scale input against the _sensitivity setting and multiply that against the _smoothing value.
		mouseDelta = Vector2.Scale(mouseDelta, new Vector2(_sensitivity.x, _sensitivity.y));

		// NOTE: you dont multiply sens for deltatime because you want the mouse look 
		// to be frame dependent (to avoid jerky movements when fps are low).

		// Interpolate mouse movement over time to apply _smoothing delta.
		// TODO once decided the right _smoothing, multiply it instead of dividing
		_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / _smoothing.x);
		_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / _smoothing.y);

		// Find the absolute mouse movement value from point zero.
		_mouseAbsolute += _smoothMouse;
		// Clamp and apply the local x value first, so as not to be affected by world transforms.
		if (_clampInDegrees.x < 360)
			_mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -_clampInDegrees.x * 0.5f, _clampInDegrees.x * 0.5f);

		// Then clamp and apply the global y value.
		if (_clampInDegrees.y < 360)
			_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -_clampInDegrees.y * 0.5f, _clampInDegrees.y * 0.5f);

		// Apply the absolute rotation around x axis
		transform.localRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, _targetOrientation * Vector3.right) * _targetOrientation;

		// Apply the absolute rotation around y axis
		// If there's a character body that acts as a parent to the camera
		if (characterBody)
		{
			Quaternion yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
			characterBody.transform.localRotation = yRotation * _targetCharacterOrientation;
		}
		else
		{
			Quaternion yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
			transform.localRotation *= yRotation;
		}
	}
}
