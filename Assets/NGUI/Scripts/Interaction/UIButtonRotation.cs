﻿using UnityEngine;

/// <summary>
/// Simple example script of how a button can be rotated visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Rotation")]
public class UIButtonRotation : MonoBehaviour
{
	public Transform tweenTarget;
	public Vector3 hover = Vector3.zero;
	public Vector3 pressed = Vector3.zero;
	public float duration = 0.2f;

	Quaternion mRot;

	void Start ()
	{
		if (tweenTarget == null) tweenTarget = transform;
		mRot = tweenTarget.localRotation;
	}

	void OnPress (bool isPressed)
	{
		if (enabled) TweenRotation.Begin(tweenTarget.gameObject, duration, isPressed ? mRot * Quaternion.Euler(pressed) : mRot).method = Tweener.Method.EaseInOut;
	}

	void OnHover (bool isOver)
	{
		if (enabled) TweenRotation.Begin(tweenTarget.gameObject, duration, isOver ? mRot * Quaternion.Euler(hover) : mRot).method = Tweener.Method.EaseInOut;
	}
}