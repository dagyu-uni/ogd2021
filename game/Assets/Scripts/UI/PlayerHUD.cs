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

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Slider _staminaSlider = null;
    [SerializeField] private Text _interactionText = null;
	[SerializeField] private Text _eventText = null;
	public List<Color> eventColors = null;
	[SerializeField] private Image _screenFade = null;
    [SerializeField] private Text _missionText = null;
    [SerializeField] private float _missionTextDisplayTime = 3.0f;

	// Inventory system for items interface elements
	[Header("Inventory")]
	[SerializeField] private GameObject _inventoryUI = null;
	[SerializeField] private Image _inventoryIcon = null;
	[SerializeField] private List<InventorySlot> _inventorySlots = new List<InventorySlot>();
	[SerializeField] private Image _inventoryTooltip = null;
	[SerializeField] private Text _capacityText = null;

	//TODO skills

	// Internals
	private float _currentFadeLevel = 1.0f;
    private IEnumerator _coroutine = null;

	// Properties
	public string CapacityText { set { _capacityText.text = value; } }
	public GameObject InventoryUI { get { return _inventoryUI; } }
	public Image InventoryIcon { get { return _inventoryIcon; } }
	public Image InventoryTooltip { get { return _inventoryTooltip; } }

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

		// Initialize inventory slots
		InitializeInventorySlots();
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

	private void ThrowCollectable(Collectable collectable)
	{
		if (collectable == null) return; 

		// Set the gameobject transform properly
		GameObject obj = collectable.gameObject;
		PlayerInfo info = GameManager.Instance.GetPlayerInfo(collectable._collectorID);
		Transform cameraTransform = info.characterManager.Camera.transform;
		obj.transform.position = cameraTransform.position + cameraTransform.forward * 1.5f;
		obj.transform.rotation = Quaternion.identity;
		obj.SetActive(true);
		collectable.rb.velocity = cameraTransform.forward * 1.5f;

		info.characterManager.SubtractCollectable(collectable.name);
	}

	// Refreshes the values of the UI
	public void RefreshHUD(CharacterManager cm)
    {
        if (cm == null) return;
		// refresh stamina slider value
		_staminaSlider.value = cm.Controller.Stamina / cm.Controller.MaxStamina;
	}

	// Give textual feedback and manage the inventory UI.
	public void RefreshCollectables(List<Collectable> inventory, Collectable collectable, bool added)
	{
		// Manage the inventory UI and refresh tooltip text
		for (int i = 0; i < inventory.Count; i++)
		{
			_inventorySlots[i].collectable = inventory[i];
			_inventorySlots[i].icon.sprite = inventory[i].icon;
			string tooltip = inventory[i].tooltipString + "\nClick To Drop";
			_inventorySlots[i].TooltipTrigger.tooltipString = tooltip;
		}

		for (int i = inventory.Count; i < inventory.Capacity; i++)
		{
			_inventorySlots[i].collectable = null;
			_inventorySlots[i].icon.sprite = null;
			_inventorySlots[i].TooltipTrigger.tooltipString = "";
		}

		_capacityText.text = inventory.Count.ToString() + " / " + inventory.Capacity.ToString();

		// Give textual feedback 
		string collected = "";
		Color color;
		if (added)
		{
			collected = "You picked the " + collectable.name + "!";
			color = eventColors[1];
		}
		else
		{
			collected = "You dropped the " + collectable.name + ".";
			color = eventColors[0];
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

	// used to notify any kind of dynamic event to the player
	public IEnumerator SetEventText(string text, Color color)
	{
		if (_eventText)
		{
			_eventText.text = text;
			_eventText.color = color;
			yield return new WaitForSeconds(3.0f);
			_eventText.text = null;
		}
	}

    public void Fade(float seconds, ScreenFadeType fadeType)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);

        float targetFade = fadeType == ScreenFadeType.FadeIn ? 0.0f : 1.0f;

        _coroutine = FadeInternal(seconds, targetFade);
        StartCoroutine(_coroutine);
    }

    private IEnumerator FadeInternal(float seconds, float targetFade)
    {
        if (!_screenFade) yield break;

        float timer = 0f;
        float srcFade = _currentFadeLevel;
        Color oldColor = _screenFade.color;
        if (seconds < 0.1f) seconds = 0.1f;

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
}
