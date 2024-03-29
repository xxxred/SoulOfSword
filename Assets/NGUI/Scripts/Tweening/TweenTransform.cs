﻿using UnityEngine;

/// <summary>
/// Tween the object's position, rotation and scale.
/// </summary>

[AddComponentMenu("NGUI/Tween/Transform")]
public class TweenTransform : Tweener
{
	public Transform from;
	public Transform to;

	Transform mTrans;

	void Awake () { mTrans = transform; }

	override protected void OnUpdate (float factor)
	{
		if (from != null && to != null)
		{
			mTrans.position = from.position * (1f - factor) + to.position * factor;
			mTrans.localScale = from.localScale * (1f - factor) + to.localScale * factor;
			mTrans.rotation = Quaternion.Slerp(from.rotation, to.rotation, factor);
		}
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenTransform Begin (GameObject go, float duration, Transform from, Transform to)
	{
		TweenTransform comp = Tweener.Begin<TweenTransform>(go, duration);
		comp.from = from;
		comp.to = to;
		return comp;
	}
}