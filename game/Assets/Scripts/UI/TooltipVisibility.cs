using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// The objective of this script is to keep the tooltip visible
// even when the cursor goes over it (exiting from the component
// which activated it initially).

// Assign this only to a tooltip interface (Image)

public class TooltipVisibility : EventTrigger
{
	private Image _tooltip = null;

	private bool onTooltip = false;

	[HideInInspector] public TooltipTrigger _currentTrigger = null;


	private void Start()
	{
		_tooltip = GetComponent<Image>();
	}

	private void Update()
	{
		if (gameObject.activeInHierarchy && !_currentTrigger.onButton && !onTooltip)
		{
			gameObject.SetActive(false);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		onTooltip = true;
		_tooltip.gameObject.SetActive(true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		onTooltip = false;
		_tooltip.gameObject.SetActive(false);
	}
}
