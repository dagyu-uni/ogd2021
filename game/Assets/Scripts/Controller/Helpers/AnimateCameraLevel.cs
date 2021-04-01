using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCameraLevel : MonoBehaviour
{
    public float adjustSpeed = 15f;
    private float _lerpFactor = 1f;
    private Vector3 _offset;

    private void Start()
    {
        _offset = transform.localPosition;
    }

    public void UpdateLevel(float level, float crouchCamAdjust)
    {
        if (level == transform.localPosition.y)
        {
            _lerpFactor = 1f;
            return;
        }    
        else
            _lerpFactor = _lerpFactor == 1f ? 0f : Mathf.Min(1.0f, _lerpFactor + Time.deltaTime * adjustSpeed);

        Vector3 crouchlevel = _offset;
        crouchlevel.y += crouchCamAdjust;

        if (level == crouchCamAdjust)    // crouching
            transform.localPosition = Vector3.Lerp(_offset, crouchlevel, _lerpFactor);
        else    // uncrouching
            transform.localPosition = Vector3.Lerp(crouchlevel, _offset, _lerpFactor);
    }
}
