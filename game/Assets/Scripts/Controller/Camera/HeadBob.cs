using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Contains all the mechanics related to bobs
// Standard bob, jump bob, callbacks etc.

public enum BobCallbackType { Horizontal, Vertical }

// Delegates
public delegate void BobCallback();

[System.Serializable]
public class BobEvent
{
	public float time = 0.0f;
	public BobCallbackType type = BobCallbackType.Vertical;

	public BobCallback function = null;
}

[System.Serializable]
public class HeadBob
{
	// sin curve for head bob
	public AnimationCurve bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
														 new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
														 new Keyframe(2f, 0f));

	// the higher the base interval the slower is the curve timeline
	// this works like a master speed for all the head bobs
	[SerializeField] private float _baseInterval = 1.0f;

	public float crouchingSpeed = 0.5f;
	public float[] crouchingMultipliers = { 0.02f, 0.04f };
	public float walkingSpeed = 1.0f;
	public float[] walkingMultipliers = { 0.04f, 0.08f };
	public float sprintingSpeed = 2.0f;
	public float[] sprintingMultipliers = { 0.08f, 0.016f };

	// this ratio shouldn't be modified for standard behaviors (walking, running, etc.)
	private float _verticalToHorizontalRatio = 2.0f;
	private float _xPlayHead;
	private float _yPlayHead;
	private float _curveEndTime;
	private float _prevXPlayHead;
	private float _prevYPlayHead;
	private List<BobEvent> _events = new List<BobEvent>();


	public void Initialize()
	{
		_curveEndTime = bobcurve[bobcurve.length - 1].time;
		_xPlayHead = 0.0f;
		_yPlayHead = 0.0f;
		_prevXPlayHead = 0.0f;
		_prevYPlayHead = 0.0f;
	}

	public void RegisterEventCallback(float time, BobCallback function, BobCallbackType type)
	{
		BobEvent bobEvent = new BobEvent();
		bobEvent.time = time;
		bobEvent.function = function;
		bobEvent.type = type;
		_events.Add(bobEvent);
		// same can be done with lambda functions
		// e.g. Sort((a,b)=>e1.time.CompareTo(e2.time));
		_events.Sort(
			delegate (BobEvent e1, BobEvent e2)
			{
				// ascending order. Invert the arguments order to have descending order
				// e.g.  e2.time.CompareTo(e1.time);
				return e1.time.CompareTo(e2.time);
			}
		);
	}

	public Vector3 GetVectorOffset(float speed, float[] multipliers)
	{
		_xPlayHead += (speed * Time.deltaTime) / _baseInterval;
		_yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticalToHorizontalRatio;

		// rewind the timeline
		if (_xPlayHead > _curveEndTime)
			_xPlayHead -= _curveEndTime;

		if (_yPlayHead > _curveEndTime)
			_yPlayHead -= _curveEndTime;

		// Process Events
		for (int i = 0; i < _events.Count; i++)
		{
			BobEvent ev = _events[i];
			if (ev != null)
			{
				if (ev.type == BobCallbackType.Vertical)
				{
					if (_prevYPlayHead < ev.time && _yPlayHead >= ev.time ||
						(_prevYPlayHead > _yPlayHead && (_prevYPlayHead < ev.time || _yPlayHead >= ev.time)))
					{
						ev.function();
					}
				}
				else
				{
					if (_prevXPlayHead < ev.time && _xPlayHead >= ev.time ||
											_prevXPlayHead > _xPlayHead && (_prevXPlayHead < ev.time || _xPlayHead >= ev.time))
					{
						ev.function();
					}
				}
			}
		}

		// apply the bob
		float xPos = bobcurve.Evaluate(_xPlayHead) * multipliers[0];
		float yPos = bobcurve.Evaluate(_yPlayHead) * multipliers[1];

		_prevXPlayHead = _xPlayHead;
		_prevYPlayHead = _yPlayHead;

		return new Vector3(xPos, yPos, 0.0f);
	}
}
