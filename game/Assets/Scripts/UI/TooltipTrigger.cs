using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Allows to trigger a tooltip if properly initialized (externally from script)
public class TooltipTrigger : EventTrigger
{
	[HideInInspector] public Image tooltipImage = null;
	[HideInInspector] public string tooltipString = "";
	[HideInInspector] public bool onButton = false;

	private TooltipVisibility _visibility = null;
	private Coroutine _coroutine = null;
	private Text tooltipText = null;

	private void Start()
	{
		tooltipText = tooltipImage.GetComponentInChildren<Text>();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (_visibility == null)
			_visibility = tooltipImage.GetComponent<TooltipVisibility>();

		_coroutine = StartCoroutine(ShowTooltip(eventData));

		_visibility._currentTrigger = this;
		onButton = true;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (_visibility.gameObject.activeInHierarchy)
		{
			onButton = false;
			return;
		}

		StopCoroutine(_coroutine);
		tooltipImage.gameObject.SetActive(false);
	}

	private IEnumerator ShowTooltip(PointerEventData eventData)
	{
		yield return new WaitForSeconds(1.0f);

		// Set the correct position
		tooltipImage.transform.position = eventData.position + Vector2.up;
		// Set the text
		tooltipText.text = tooltipString;

		if (tooltipString != "")
			tooltipImage.gameObject.SetActive(true);
	}
}
