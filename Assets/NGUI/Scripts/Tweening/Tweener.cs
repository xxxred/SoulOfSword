﻿using UnityEngine;

/// <summary>
/// Base class for all tweening operations.
/// </summary>

public abstract class Tweener : MonoBehaviour
{
	public enum Method
	{
		Linear,
		EaseIn,
		EaseOut,
		EaseInOut,
	}

	public enum Style
	{
		Once,
		Loop,
		PingPong,
	}

	/// <summary>
	/// Tweening method used.
	/// </summary>

	public Method method = Method.Linear;

	/// <summary>
	/// Does it play once? Does it loop?
	/// </summary>

	public Style style = Style.Once;

	/// <summary>
	/// How long is the duration of the tween?
	/// </summary>

	public float duration = 1f;

	/// <summary>
	/// Used by buttons and tween sequences. Group of '0' means not in a sequence.
	/// </summary>

	public int tweenGroup = 0;

	/// <summary>
	/// Target used with 'callWhenFinished', or this game object if none was specified.
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Name of the function to call when the tween finishes.
	/// </summary>

	public string callWhenFinished;

	float mDuration = 0f;
	float mAmountPerDelta = 1f;
	float mFactor = 0f;

	/// <summary>
	/// Amount advanced per delta time.
	/// </summary>

	float amountPerDelta
	{
		get
		{
			if (mDuration != duration)
			{
				mDuration = duration;
				mAmountPerDelta = Mathf.Abs((duration > 0f) ? 1f / duration : 1000f);
			}
			return mAmountPerDelta;
		}
	}

	/// <summary>
	/// Update the tweening factor and call the virtual update function.
	/// </summary>

	void Update ()
	{
		// Advance the sampling factor
		mFactor += amountPerDelta * Time.deltaTime;

		// Loop style simply resets the play factor after it exceeds 1.
		if (style == Style.Loop)
		{
			if (mFactor > 1f)
			{
				mFactor -= Mathf.Floor(mFactor);
			}
		}
		else if (style == Style.PingPong)
		{
			// Ping-pong style reverses the direction
			if (mFactor > 1f)
			{
				mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
				mAmountPerDelta = -mAmountPerDelta;
			}
			else if (mFactor < 0f)
			{
				mFactor = -mFactor;
				mFactor -= Mathf.Floor(mFactor);
				mAmountPerDelta = -mAmountPerDelta;
			}
		}

		// Calculate the sampling value
		float val = Mathf.Clamp01(mFactor);

		if (method == Method.EaseIn)
		{
			val = 1f - Mathf.Sin(0.5f * Mathf.PI * (1f - val));
		}
		else if (method == Method.EaseOut)
		{
			val = Mathf.Sin(0.5f * Mathf.PI * val);
		}
		else if (method == Method.EaseInOut)
		{
			const float pi2 = Mathf.PI * 2f;
			val = val - Mathf.Sin(val * pi2) / pi2;
		}

		// Call the virtual update
		OnUpdate(val);

		// If the factor goes out of range and this is a one-time tweening operation, disable the script
		if (style == Style.Once && (mFactor > 1f || mFactor < 0f))
		{
			mFactor = Mathf.Clamp01(mFactor);

			if (string.IsNullOrEmpty(callWhenFinished))
			{
				enabled = false;
			}
			else
			{
				// Notify the event listener target
				GameObject go = eventReceiver;
				if (go == null) go = gameObject;
				go.SendMessage(callWhenFinished, this, SendMessageOptions.DontRequireReceiver);

				// Disable this script unless the SendMessage function above changed something
				if (mFactor == 1f && mAmountPerDelta > 0f || mFactor == 0f && mAmountPerDelta < 0f)
				{
					enabled = false;
				}
			}
		}
	}

	/// <summary>
	/// Manually activate the tweening process, reversing it if necessary.
	/// </summary>

	public void Play (bool forward)
	{
		mAmountPerDelta = Mathf.Abs(amountPerDelta);
		if (!forward) mAmountPerDelta = -mAmountPerDelta;
		enabled = true;
	}

	[System.Obsolete("Use Tweener.Play instead")]
	public void Animate (bool forward) { Play(forward); }

	/// <summary>
	/// Manually reset the tweener's state to the beginning.
	/// </summary>

	public void Reset() { mFactor = (mAmountPerDelta < 0f) ? 1f : 0f; }

	/// <summary>
	/// Manually start the tweening process, reversing its direction.
	/// </summary>

	public void Toggle ()
	{
		if (mFactor > 0f)
		{
			mAmountPerDelta = -amountPerDelta;
		}
		else
		{
			mAmountPerDelta = Mathf.Abs(amountPerDelta);
		}
		enabled = true;
	}

	/// <summary>
	/// Actual tweening logic should go here.
	/// </summary>

	abstract protected void OnUpdate (float factor);

	/// <summary>
	/// Starts the tweening operation.
	/// </summary>

	static public T Begin<T> (GameObject go, float duration) where T : Tweener
	{
		T comp = go.GetComponent<T>();
#if UNITY_FLASH
		if ((object)comp == null) comp = (T)go.AddComponent<T>();
#else
		if (comp == null) comp = go.AddComponent<T>();
#endif
		comp.duration = duration;
		comp.mFactor = 0f;
		comp.style = Style.Once;
		comp.enabled = true;
		return comp;
	}
}