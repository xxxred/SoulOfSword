﻿using UnityEngine;

/// <summary>
/// Tween the object's color.
/// </summary>

[AddComponentMenu("NGUI/Tween/Color")]
public class TweenColor : Tweener
{
	public Color from = Color.white;
	public Color to = Color.white;

	Transform mTrans;
	UIWidget mWidget;
	Material mMat;
	Light mLight;

	/// <summary>
	/// Current color.
	/// </summary>

	public Color color
	{
		get
		{
			if (mWidget != null) return mWidget.color;
			if (mLight != null) return mLight.color;
			if (mMat != null) return mMat.color;
			return Color.black;
		}
		set
		{
			if (mWidget != null) mWidget.color = value;
			if (mMat != null) mMat.color = value;

			if (mLight != null)
			{
				mLight.color = value;
				mLight.enabled = (value.r + value.g + value.b) > 0.01f;
			}
		}
	}

	/// <summary>
	/// Find all needed components.
	/// </summary>

	void Awake ()
	{
		mWidget = GetComponent<UIWidget>();
		Renderer ren = renderer;
		if (ren != null) mMat = ren.material;
		mLight = light;
	}

	/// <summary>
	/// Interpolate and update the color.
	/// </summary>

	override protected void OnUpdate (float factor) { color = from * (1f - factor) + to * factor; }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenColor Begin (GameObject go, float duration, Color color)
	{
		TweenColor comp = Tweener.Begin<TweenColor>(go, duration);
		comp.from = comp.color;
		comp.to = color;
		return comp;
	}
}