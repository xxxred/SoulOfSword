﻿using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified
/// target when the script receives OnHover or OnPress messages from UICamera.
/// DEPRECATED: This script has been deprecated as of version 1.30.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/Send State")]
public class UISendState : UISend
{
	public int normalState	= 0;
	public int hoverState	= 1;
	public int pressedState = 2;

	void Start ()
	{
		Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " uses a deprecated script: " + GetType() +
			"\nConsider switching to UIButtonScale, UIButtonColor, UIButtonOffset or UIButtonTween instead.", this);
	}

	void OnHover (bool isOver)
	{
		Send(isOver ? hoverState : normalState);
	}

	void OnPress (bool pressed)
	{
		Send(pressed ? pressedState : normalState);
	}
}