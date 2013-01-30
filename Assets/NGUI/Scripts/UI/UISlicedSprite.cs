﻿using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 9-sliced widget component used to draw large widgets using small textures.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Sliced)")]
public class UISlicedSprite : UISprite
{
#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	public bool mFillCenter = true;
#else
	[SerializeField] bool mFillCenter = true;
#endif

	Rect mInner;
	Rect mInnerUV;
	Vector3 mScale = Vector3.one;

	/// <summary>
	/// Inner set of UV coordinates.
	/// </summary>

	public Rect innerUV { get { UpdateUVs(); return mInnerUV; } }

	/// <summary>
	/// Whether the center part of the sprite will be filled or not. Turn it off if you want only to borders to show up.
	/// </summary>

	public bool fillCenter { get { return mFillCenter; } set { if (mFillCenter != value) { mFillCenter = value; MarkAsChanged(); } } }

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

	override public void UpdateUVs()
	{
		Init();

		Texture2D tex = mainTexture;

		if (tex != null && sprite != null)
		{
			if (mInner != mSprite.inner || mOuter != mSprite.outer || cachedTransform.localScale != mScale)
			{
				mInner = mSprite.inner;
				mOuter = mSprite.outer;
				mScale = cachedTransform.localScale;

				mInnerUV = mInner;
				mOuterUV = mOuter;

				if (atlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					mOuterUV = NGUIMath.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
					mInnerUV = NGUIMath.ConvertToTexCoords(mInnerUV, tex.width, tex.height);
				}
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Sliced sprite shouldn't inherit the sprite's changes to this function.
	/// </summary>

	override public void MakePixelPerfect ()
	{
		Vector3 pos = cachedTransform.localPosition;
		pos.x = Mathf.RoundToInt(pos.x);
		pos.y = Mathf.RoundToInt(pos.y);
		pos.z = Mathf.RoundToInt(pos.z);
		cachedTransform.localPosition = pos;

		Vector3 scale = cachedTransform.localScale;
		scale.x = Mathf.RoundToInt(scale.x * 0.5f) << 1;
		scale.y = Mathf.RoundToInt(scale.y * 0.5f) << 1;
		scale.z = 1f;
		cachedTransform.localScale = scale;
	}

	/// <summary>
	/// Draw the widget.
	/// </summary>

	override public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		if (mOuterUV == mInnerUV)
		{
			base.OnFill(verts, uvs, cols);
			return;
		}

		Vector2[] v  = new Vector2[4];
		Vector2[] uv = new Vector2[4];

		Texture tex = mainTexture;

		v[0] = Vector2.zero;
		v[1] = Vector2.zero;
		v[2] = new Vector2(1f, -1f);
		v[3] = new Vector2(1f, -1f);

		if (tex != null)
		{
			float borderLeft	= mInnerUV.xMin - mOuterUV.xMin;
			float borderRight	= mOuterUV.xMax - mInnerUV.xMax;
			float borderTop		= mOuterUV.yMin - mInnerUV.yMin;
			float borderBottom	= mInnerUV.yMax - mOuterUV.yMax;

			Vector3 scale = cachedTransform.localScale;
			scale.x = Mathf.Max(0f, scale.x);
			scale.y = Mathf.Max(0f, scale.y);

			Vector2 sz = new Vector2(scale.x / tex.width, scale.y / tex.height);
			Vector2 tl = new Vector2(borderLeft / sz.x, borderTop / sz.y);
			Vector2 br = new Vector2(borderRight / sz.x, borderBottom / sz.y);

			Pivot pv = pivot;

			// We don't want the sliced sprite to become smaller than the summed up border size
			if (pv == Pivot.Right || pv == Pivot.TopRight || pv == Pivot.BottomRight)
			{
				v[0].x = Mathf.Min(0f, 1f - (br.x + tl.x));
				v[1].x = v[0].x + tl.x;
				v[2].x = v[0].x + Mathf.Max(tl.x, 1f - br.x);
				v[3].x = v[0].x + Mathf.Max(tl.x + br.x, 1f);
			}
			else
			{
				v[1].x = tl.x;
				v[2].x = Mathf.Max(tl.x, 1f - br.x);
				v[3].x = Mathf.Max(tl.x + br.x, 1f);
			}

			if (pv == Pivot.Bottom || pv == Pivot.BottomLeft || pv == Pivot.BottomRight)
			{
				v[0].y = Mathf.Max(0f, -1f - (br.y + tl.y));
				v[1].y = v[0].y + tl.y;
				v[2].y = v[0].y + Mathf.Min(tl.y, -1f - br.y);
				v[3].y = v[0].y + Mathf.Min(tl.y + br.y, -1f);
			}
			else
			{
				v[1].y = tl.y;
				v[2].y = Mathf.Min(tl.y, -1f - br.y);
				v[3].y = Mathf.Min(tl.y + br.y, -1f);
			}

			uv[0] = new Vector2(mOuterUV.xMin, mOuterUV.yMax);
			uv[1] = new Vector2(mInnerUV.xMin, mInnerUV.yMax);
			uv[2] = new Vector2(mInnerUV.xMax, mInnerUV.yMin);
			uv[3] = new Vector2(mOuterUV.xMax, mOuterUV.yMin);
		}
		else
		{
			// No texture -- just use zeroed out texture coordinates
			for (int i = 0; i < 4; ++i) uv[i] = Vector2.zero;
		}

		for (int x = 0; x < 3; ++x)
		{
			int x2 = x + 1;

			for (int y = 0; y < 3; ++y)
			{
				if (!mFillCenter && x == 1 && y == 1) continue;

				int y2 = y + 1;

				verts.Add(new Vector3(v[x2].x, v[y].y, 0f));
				verts.Add(new Vector3(v[x2].x, v[y2].y, 0f));
				verts.Add(new Vector3(v[x].x, v[y2].y, 0f));
				verts.Add(new Vector3(v[x].x, v[y].y, 0f));

				uvs.Add(new Vector2(uv[x2].x, uv[y].y));
				uvs.Add(new Vector2(uv[x2].x, uv[y2].y));
				uvs.Add(new Vector2(uv[x].x, uv[y2].y));
				uvs.Add(new Vector2(uv[x].x, uv[y].y));

				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
			}
		}
	}
}