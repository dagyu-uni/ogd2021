using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaMask : MonoBehaviour
{
	public Image mask;

	private float _originalSize;

	void Start()
	{
		//_originalSize = mask.rectTransform.rect.width;
		_originalSize = 270.72f;
	}

	public void SetValue(float value)
	{
		mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _originalSize * value);
	}
}
