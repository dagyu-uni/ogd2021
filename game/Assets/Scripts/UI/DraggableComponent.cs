using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableComponent : EventTrigger
{
	// Is the component being dragged
	private bool _isDragging = false;
	private Vector2 _currentPivot;

	private RectTransform _rect = null;
	private float _windowWidth;
	private float _windowHeight;

	private Vector2 _screenSize;

	private void Start()
	{
		// expressed in pixels (0,0 bottom left / 1,1 top right).
		_screenSize = new Vector2(Screen.width, Screen.height);
	}

	private void Update()
	{
		if (_isDragging)
		{
			// Limit the mouse position to avoid the window to go out from the screen
			float minX = _currentPivot.x * _windowWidth;
			float minY = _currentPivot.y * _windowHeight;
			float maxX = _screenSize.x - ((1.0f - _currentPivot.x) * _windowWidth);
			float maxY = _screenSize.y - ((1.0f - _currentPivot.y) * _windowHeight);

			float mousePosX = Mathf.Clamp(Input.mousePosition.x, minX, maxX);
			float mousePosY = Mathf.Clamp(Input.mousePosition.y, minY, maxY);

			transform.position = new Vector2(mousePosX, mousePosY);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (_rect == null)
		{
			_rect = eventData.selectedObject.GetComponent<RectTransform>();
			_windowWidth = (_rect.anchorMax.x - _rect.anchorMin.x) * _screenSize.x;
			_windowHeight = (_rect.anchorMax.y - _rect.anchorMin.y) * _screenSize.y;
		}

		// Check if you have clicked another selectable UI object
		if (!_rect.gameObject.CompareTag("InventoryUI"))
		{
			_rect = null;
			return;
		}

		// Remove the dragging snapping effect (pivot teleporting where clicked)
		// Cache the distance from the pivot
		Vector2 distFromPivot = (Vector2)_rect.transform.position - eventData.pressPosition;
		// Supposing that the window is stretched using its anchors, find its width and height.

		// Percentage of how distant you clicked from the pivot.
		Vector2 pointerOffset = new Vector2(distFromPivot.x / (_windowWidth * 0.01f),
											distFromPivot.y / (_windowHeight * 0.01f));

		// Scale the offset 0-100 percentage to a 0-1 one.
		_rect.pivot -= pointerOffset * 0.01f;
		_currentPivot = _rect.pivot;

		_isDragging = true;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		_isDragging = false;
	}
}
