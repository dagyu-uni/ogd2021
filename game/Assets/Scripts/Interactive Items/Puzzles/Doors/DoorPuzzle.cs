using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorPuzzle : MonoBehaviour
{
	public float sliderSpeed = 1.0f;
	public float sliderThreshold = 0.1f;
	public float circleSpeed = 1.0f;
	public float circleThreshold = 45.0f;
	public float circleMinCd = 1.0f;
	public float circleMaxCd = 6.0f;
	public float lockSpeed = 120.0f;

	[SerializeField] private InteractiveDoor _door = null;
	[SerializeField] private Slider _slider = null;
	[SerializeField] private Image _sliderBackground = null;
	[SerializeField] private Image _safeCircle = null;
	[SerializeField] private Image _safeLock = null;
	[SerializeField] private Button _unlockButton = null;
	[SerializeField] private List<SliderSprite> _sliderSprites = new List<SliderSprite>();
	[SerializeField] private AudioCollection _lockpickBroken = null;

	private bool _handleFlag = true;
	// if the player has tried to solve it
	private bool _isSubmitted = false;
	private int random = 0;
	private float _circleCooldown = 0.0f;
	private float _circleTime = 0.0f;
	private float _circleRotation = 0.0f;
	private int _circleSign = 1;

	private void Start()
	{
		ResetPuzzle();
	}

	private void Update()
	{
		if (_isSubmitted)
			return;

		// automatically update the handle of the slider (true -> right, false -> left)
		if (_handleFlag)
		{
			_slider.value = Mathf.Min(1.0f, _slider.value + Time.deltaTime * sliderSpeed);
			_handleFlag = _slider.value != 1.0f;
		}
		else
		{
			_slider.value = Mathf.Max(0.0f, _slider.value - Time.deltaTime * sliderSpeed);
			_handleFlag = _slider.value == 0.0f;
		}

		// automatically update the safe circle
		_circleTime += Time.deltaTime;
		if (_circleTime >= _circleCooldown)
		{
			_circleTime = 0.0f;
			_circleCooldown = Random.Range(circleMinCd, circleMaxCd);
			_circleSign *= -1;
		}
		_circleRotation += Time.deltaTime * circleSpeed * _circleSign;
		_safeCircle.transform.rotation = Quaternion.AngleAxis(_circleRotation, Vector3.forward);

		// rotate the door lock
		float horizontal = Input.GetAxis("Horizontal");
		float eulerZ = _safeLock.transform.rotation.eulerAngles.z;
		if (horizontal < 0.0f)
		{
			// rotate counter clockwise
			_safeLock.transform.rotation = Quaternion.AngleAxis(eulerZ + Time.deltaTime * lockSpeed, Vector3.forward);
		}
		else if (horizontal > 0.0f)
		{
			//rotate clockwise
			_safeLock.transform.rotation = Quaternion.AngleAxis(eulerZ - Time.deltaTime * lockSpeed, Vector3.forward);
		}
	}

	// listener for the interface button
	private void TryToUnlock()
	{
		_isSubmitted = true;
		float center = _sliderSprites[random].center;
		// if the handle is in the correct threshold you solved it
		float lockZ = _safeLock.transform.rotation.eulerAngles.z;
		float circleZ = _safeCircle.transform.rotation.eulerAngles.z;
		if (_slider.value < center + sliderThreshold && _slider.value > center - sliderThreshold &&
			lockZ < circleZ + circleThreshold && lockZ > circleZ - circleThreshold)
		{
			StartCoroutine(TryOpenDoor(true));
		}
		else
		{
			//broken lock pick sound
			if (_lockpickBroken)
			{
				AudioManager.Instance.PlayOneShotSound(
					_lockpickBroken.MixerGroupName,
					_lockpickBroken.AudioClip.name,
					_door.transform.position,
					_lockpickBroken.Volume,
					_lockpickBroken.SpatialBlend,
					_lockpickBroken.Priority
				);
			}
			StartCoroutine(TryOpenDoor(false));
		}
	}

	private IEnumerator TryOpenDoor(bool success)
	{
		yield return new WaitForSeconds(1.0f);
		_door.TryOpenDoor(success);
		gameObject.SetActive(false);
	}

	public void ResetPuzzle()
	{
		_isSubmitted = false;
		// randomly set a sprite for the slider
		random = Random.Range(0, _sliderSprites.Count);
		_sliderBackground.sprite = _sliderSprites[random].sprite;
		_circleCooldown = Random.Range(circleMinCd, circleMaxCd);
		_circleTime = 0.0f;
		_circleRotation = 0.0f;
		_circleSign = 1;

		_unlockButton.onClick.RemoveAllListeners();
		_unlockButton.onClick.AddListener(TryToUnlock);
	}
}

[System.Serializable]
public class SliderSprite
{
	public Sprite sprite = null;
	public float center = 0.0f;
}
