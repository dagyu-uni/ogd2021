using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ScreenFadeType { FadeIn, FadeOut };

[System.Serializable]
public class InventorySlot
{
	// actual visible slot image
	public Image icon;

	// collectable that fills the slot
	[HideInInspector] public Collectable collectable = null;
	// initialized in the PlayerHUD Start function
	[HideInInspector] public Button _throwButton = null;

	private TooltipTrigger _tooltipTrigger = null;

	public TooltipTrigger TooltipTrigger
	{
		get { return _tooltipTrigger; }
		set { _tooltipTrigger = value; }
	}
}

[System.Serializable]
public class SkillSlot
{
	public Image icon;
	public Image mask;
}

public class PlayerHUD : MonoBehaviour
{
	[SerializeField] private Text _remainingTimeText = null;
	//[SerializeField] private Slider _staminaSlider = null;
	[SerializeField] private StaminaMask _staminaMask = null;
	[SerializeField] private Text _interactionText = null;
	[SerializeField] private Text _eventText = null;
	[SerializeField] private Text _captureText = null;
	public List<Color> eventColors = null;
	[SerializeField] private Image _screenFade = null;
	[SerializeField] private Text _missionText = null;
	[SerializeField] private float _missionTextDisplayTime = 3.0f;

	// Pause Menu
	[Header("Pause Menu")]
	[SerializeField] private GameObject _pauseInterface = null;
	[SerializeField] private Button _resumeButton = null;
	[SerializeField] private Button _quitButton = null;

	// Inventory system for items interface elements
	[Header("Inventory")]
	[SerializeField] private GameObject _inventoryUI = null;
	[SerializeField] private Image _inventoryIcon = null;
	[SerializeField] private List<InventorySlot> _inventorySlots = new List<InventorySlot>();
	[SerializeField] private Image _inventoryTooltip = null;
	[SerializeField] private Text _capacityText = null;

	// Skills
	[Header("Skills")]
	[SerializeField] private List<SkillSlot> _skillSlots = new List<SkillSlot>();
	// if you still don't have that skill, show an empty icon
	[SerializeField] private Sprite _emptyIcon = null;
	[SerializeField] private GameObject _eyeUI = null;

	// Internals
	private CharacterManager _charManager = null;
	private float _currentFadeLevel = 1.0f;
	private IEnumerator _coroutine = null;

	// Properties
	public CharacterManager CharManager { set { _charManager = value; } }
	public int SkillsCapacity { get { return _skillSlots.Count; } }
	public int InventoryCapacity { get { return _inventorySlots.Count; } }
	public string CapacityText { set { _capacityText.text = value; } }
	public Text CaptureText { get { return _captureText; } }
	public GameObject EyeUI { get { return _eyeUI; } }


	void Start()
	{
		if (_screenFade)
		{
			Color color = _screenFade.color;
			color.a = _currentFadeLevel;
			_screenFade.color = color;
		}

		_eventText.text = null;

		if (_missionText)
			Invoke("HideMissionText", _missionTextDisplayTime);

		// Initialize Pause Menu
		InitializePauseMenu();

		// Initialize skills slots
		InitiliazeSkillSlots();

		// Initialize inventory slots
		InitializeInventorySlots();
	}

	private void Update()
	{
		// Refresh the match time
		_remainingTimeText.text = GameManager.Instance.FormatMatchTime();

		// Pause Menu
		HandlePauseMenu();

		// Skills (inputs, cooldowns etc.)
		HandleSkills();

		// Inventory
		HandleInventoryInput();
	}

	// Set buttons listeners
	private void InitializePauseMenu()
	{
		_resumeButton.onClick.AddListener(() => Resume());
		_quitButton.onClick.AddListener(() => Quit());
	}

	private void HandlePauseMenu()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			if (_pauseInterface.activeInHierarchy)
			{
				Resume();
			}
			else
			{
				_charManager.DisableControllerMovements();
				_charManager.DisableCameraMovements();
				_pauseInterface.SetActive(true);
				_charManager.EnableCursor();
			}
		}
	}

	public void InitiliazeSkillSlots()
	{
		for (int i = 0; i < _charManager.Skills.Count; i++)
		{
			InitSkillMask(_skillSlots[i], _charManager.Skills[i]);

			_skillSlots[i].icon.sprite = _charManager.Skills[i].sprite;

			if (_charManager.Skills[i].currentDuration == 0f)
				_skillSlots[i].icon.color = Color.white;
			else
				_skillSlots[i].icon.color = new Color(0.33f, 0.33f, 0.33f, 1f);

			// finally init the skills initial values
			_charManager.Skills[i].currentCooldown = 0.0f;
			_charManager.Skills[i].currentDuration = 0.0f;
		}
		// set the empty slots to a default state
		for (int i = _charManager.Skills.Count; i < _skillSlots.Count; i++)
		{
			_skillSlots[i].icon.color = new Color(0.33f, 0.33f, 0.33f, 1f);
			_skillSlots[i].icon.sprite = _emptyIcon;
			_skillSlots[i].mask.fillAmount = 0f;
		}
	}

	// If the skill has no durable effects it only starts a cooldown
	// otherwise it starts a duration timer and AFTER that the cooldown one.
	private void HandleSkills()
	{
		//foreach (Skill skill in _charManager.Skills)
		for (int i = 0; i < _charManager.Skills.Count; i++)
		{
			Skill skill = _charManager.Skills[i];
			// Cooldown
			if (skill.currentCooldown > 0f)
			{
				skill.currentCooldown = Mathf.Max(0.0f, skill.currentCooldown - Time.deltaTime);
			}

			// Duration
			if (skill.currentDuration > 0f)
			{
				skill.currentDuration = Mathf.Max(0.0f, skill.currentDuration - Time.deltaTime);

				if (skill.currentDuration == 0f)
				{
					skill.currentCooldown = skill.baseCooldown;
					_skillSlots[i].icon.color = Color.white;
				}

			}

			// Input
			if (Input.GetButtonDown(skill.FireButton) && skill.currentCooldown == 0f && skill.currentDuration == 0f)
			{
				skill.TriggerSkill();

				if (skill.isOneShotSkill)
				{
					skill.currentCooldown = skill.baseCooldown;
				}
				else // has a duration
				{
					skill.currentDuration = skill.baseDuration;
					_skillSlots[i].icon.color = new Color(0.33f, 0.33f, 0.33f, 1f);
				}
			}

			InitSkillMask(_skillSlots[i], skill);
		}
	}

	private void InitSkillMask(SkillSlot skillSlot, Skill skill)
	{
		Image mask = skillSlot.mask;

		// HUD cooldown feedback
		if (skill.currentCooldown != 0f)
		{
			mask.fillMethod = Image.FillMethod.Radial360;
			mask.fillOrigin = 2;
			mask.color = new Color(0.18f, 0.18f, 0.18f, 0.75f); // dark grey
			mask.fillAmount = skill.currentCooldown / skill.baseCooldown;
		}
		// HUD duration feedback
		else if (skill.currentDuration != 0f)
		{
			mask.fillMethod = Image.FillMethod.Vertical;
			mask.fillOrigin = 0;
			mask.color = new Color(0f, 1f, 0f, 0.45f); // light green
			mask.fillAmount = skill.currentDuration / skill.baseDuration;
		}
		else // both equal to 0
		{
			mask.fillAmount = 0f;
		}
	}

	// Initialize slot UI elements (throw button, tooltip)
	private void InitializeInventorySlots()
	{
		foreach (InventorySlot slot in _inventorySlots)
		{
			// throw button
			slot._throwButton = slot.icon.GetComponentInChildren<Button>();
			if (slot._throwButton != null)
			{
				slot._throwButton.onClick.AddListener(() => ThrowCollectable(slot.collectable));
			}

			// tooltip
			slot.TooltipTrigger = slot.icon.GetComponent<TooltipTrigger>();
			slot.TooltipTrigger.tooltipImage = _inventoryTooltip;
		}
	}

	private void HandleInventoryInput()
	{
		if (Input.GetButtonDown("Inventory"))
		{
			if (_inventoryUI.activeInHierarchy)
			{
				_inventoryUI.SetActive(false);
				_inventoryTooltip.gameObject.SetActive(false);
				_inventoryIcon.color = new Color32(255, 255, 255, 255);
				_charManager.EnableCameraMovements();
				_charManager.DisableCursor();
			}
			else
			{
				_inventoryUI.SetActive(true);
				_inventoryIcon.color = new Color32(170, 170, 170, 255);
				_charManager.DisableCameraMovements();
				_charManager.EnableCursor();
			}
		}
	}

	// Refreshes the values of the UI
	public void RefreshHUD(CharacterManager cm)
	{
		if (cm == null)
			return;
		// refresh stamina slider value
		//_staminaSlider.value = cm.Controller.Stamina / cm.Controller.MaxStamina;
		_staminaMask.SetValue(cm.Controller.Stamina / cm.Controller.MaxStamina);
	}

	// Give textual feedback and manage the inventory UI.
	public void RefreshCollectables(List<Collectable> inventory, Collectable collectable, ItemAction itemAction)
	{
		// Manage the inventory UI and refresh tooltip text
		for (int i = 0; i < inventory.Count; i++)
		{
			_inventorySlots[i].collectable = inventory[i];
			_inventorySlots[i].icon.sprite = inventory[i].icon;
			_inventorySlots[i].icon.color = Color.white;
			string tooltip = inventory[i].tooltipString + "\nClick To Drop";
			_inventorySlots[i].TooltipTrigger.tooltipString = tooltip;
		}

		for (int i = inventory.Count; i < inventory.Capacity; i++)
		{
			_inventorySlots[i].collectable = null;
			_inventorySlots[i].icon.sprite = null;
			_inventorySlots[i].icon.color = new Color32(142, 142, 142, 255);
			_inventorySlots[i].TooltipTrigger.tooltipString = "";
		}

		_capacityText.text = inventory.Count.ToString() + " / " + inventory.Capacity.ToString();

		// Give textual feedback 
		string collected = "";
		Color color;
		if (itemAction == ItemAction.Added)
		{
			collected = "You picked the " + collectable.name + "!";
			color = eventColors[1];
		}
		else if (itemAction == ItemAction.Throw)
		{
			collected = "You dropped the " + collectable.name + ".";
			color = eventColors[0];
		}
		else
		{
			collected = "You used the " + collectable.name + ".";
			color = eventColors[1];
		}

		StartCoroutine(SetEventText(collected, color));
	}

	// Sets the text that is displayed at the bottom of the display area.
	// It's used to display messages relating to interacting with objects.
	public void SetInteractionText(string text)
	{
		if (_interactionText)
		{
			_interactionText.text = text;

			if (text == null)
				_interactionText.gameObject.SetActive(false);
			else
				_interactionText.gameObject.SetActive(true);
		}
	}

	public void SetCaptureText(string text)
	{
		if (_captureText)
		{
			_captureText.text = text;
		}
	}

	// used to notify any kind of dynamic event to the player
	public IEnumerator SetEventText(string text, Color color)
	{
		if (_eventText)
		{
			_eventText.text = text;
			_eventText.color = color;
			yield return new WaitForSeconds(3.0f);
			if (_eventText.text == text)
				_eventText.text = null;
		}
	}

	public void Fade(float seconds, ScreenFadeType fadeType)
	{
		if (_coroutine != null)
			StopCoroutine(_coroutine);

		float targetFade = fadeType == ScreenFadeType.FadeIn ? 0.0f : 1.0f;

		_coroutine = FadeInternal(seconds, targetFade);
		StartCoroutine(_coroutine);
	}

	private IEnumerator FadeInternal(float seconds, float targetFade)
	{
		if (!_screenFade)
			yield break;

		float timer = 0f;
		float srcFade = _currentFadeLevel;
		Color oldColor = _screenFade.color;
		if (seconds < 0.1f)
			seconds = 0.1f;

		// actual fade
		while (timer < seconds)
		{
			timer += Time.deltaTime;
			_currentFadeLevel = Mathf.Lerp(srcFade, targetFade, timer / seconds);
			oldColor.a = _currentFadeLevel;
			_screenFade.color = oldColor;
			yield return null;
		}

		// be sure that the final value is the correct one
		oldColor.a = _currentFadeLevel = targetFade;
		_screenFade.color = oldColor;
	}

	// Allows to show objectives to the players.
	public void ShowMissionText(string text)
	{
		if (_missionText)
		{
			_missionText.text = text;
			_missionText.gameObject.SetActive(true);
		}
	}

	public void HideMissionText()
	{
		if (_missionText)
			_missionText.gameObject.SetActive(false);
	}


	//****************** BUTTONS METHODS ******************
	private void ThrowCollectable(Collectable collectable)
	{
		if (collectable == null)
			return;
		else if (collectable.isTreasure)
		{
			StartCoroutine(SetEventText("You can't throw away this treasure!", new Color32(180, 30, 30, 255)));
			return;
		}

		// Set the gameobject transform properly
		GameObject obj = collectable.gameObject;
		PlayerInfo info = GameManager.Instance.GetPlayerInfo(collectable.role);
		Transform cameraTransform = info.characterManager.Camera.transform;
		obj.transform.position = cameraTransform.position + cameraTransform.forward * 1.5f;
		obj.transform.rotation = Quaternion.identity;
		obj.SetActive(true);
		collectable.rb.velocity = cameraTransform.forward * 1.5f;

		info.characterManager.SubtractCollectable(collectable.name);
	}

	private void Resume()
	{
		_charManager.EnableControllerMovements();
		_charManager.EnableCameraMovements();
		_pauseInterface.SetActive(false);
		_charManager.DisableCursor();
	}

	private void Quit()
	{
		Application.Quit();
	}

	//******************
}
