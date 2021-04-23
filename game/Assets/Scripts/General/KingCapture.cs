using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingCapture : MonoBehaviour
{
	public string captureText = "";
	public LayerMask layerMask;

	[SerializeField] private Transform _kingTransform = null;
	[SerializeField] private PlayerHUD _playerHUD = null;

	private bool _wizardIsVisible = false;

	// these are just utility internals
	private float _checkCooldown = 0.2f;
	private float _inputCooldown = 0.2f;

	private void Start()
	{
		//_layerMask = LayerMask.GetMask("King", "KingBody");
		// inverse the bit mask to collide with everything minus "King/KingBody" layers.
		//_layerMask = ~_layerMask;

		_playerHUD.SetCaptureText(captureText);
	}

	private void OnTriggerEnter(Collider other)
	{
		CheckForWizard(other);
	}

	private void OnTriggerStay(Collider other)
	{
		_inputCooldown -= Time.deltaTime;
		_checkCooldown -= Time.deltaTime;

		if (_checkCooldown <= 0.0f)
		{
			CheckForWizard(other);
			_checkCooldown = 0.2f;
		}

		// Manage the actual king input to capture the wizard
		if (_wizardIsVisible && (other.gameObject.CompareTag("Wizard_1") || other.gameObject.CompareTag("Wizard_2")))
		{
			if (_inputCooldown <= 0.0f && Input.GetButton("Fire1"))
			{
				_inputCooldown = 0.2f;
				CharacterManager manager = other.GetComponent<CharacterManager>();
				GameManager.Instance.CaptureWizard(manager);
				manager.IsCaptured = true;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		// if you are not colliding a wizard, do nothing.
		if (other.gameObject.CompareTag("Wizard_1") || other.gameObject.CompareTag("Wizard_2"))
		{
			_wizardIsVisible = false;
			_playerHUD.CaptureText.gameObject.SetActive(false);
		}
	}

	private void CheckForWizard(Collider other)
	{
		// if you are not colliding a wizard, do nothing.
		if (other.gameObject.CompareTag("Wizard_1") || other.gameObject.CompareTag("Wizard_2"))
		{
			// Check that you actually see the wizard
			RaycastHit[] hits;
			Ray ray = new Ray(_kingTransform.position, other.transform.position - _kingTransform.position);
			float rayLength = Vector3.Distance(_kingTransform.position, other.transform.position);
			hits = Physics.RaycastAll(ray, rayLength, layerMask);

			// if the first hit object is the wizard than you can see him
			GameObject go = hits[0].collider.gameObject;
			if (go.CompareTag("Wizard_1") || go.CompareTag("Wizard_2"))
			{
				_wizardIsVisible = true;
				_playerHUD.CaptureText.gameObject.SetActive(true);
			}
			else
			{
				_wizardIsVisible = false;
				_playerHUD.CaptureText.gameObject.SetActive(false);
			}
		}
	}
}
