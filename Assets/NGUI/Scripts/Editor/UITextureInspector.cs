﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UITextures.
/// </summary>

[CustomEditor(typeof(UITexture))]
public class UITextureInspector : UIWidgetInspector
{
	override protected bool OnDrawProperties ()
	{
		Material mat = EditorGUILayout.ObjectField("Material", mWidget.material, typeof(Material), false) as Material;

		if (mWidget.material != mat)
		{
			Undo.RegisterUndo(mWidget, "Material Selection");
			mWidget.material = mat;
			EditorUtility.SetDirty(mWidget.gameObject);
		}
		return (mWidget.material != null);
	}

	override protected void OnDrawTexture ()
	{
		Texture2D tex = mWidget.mainTexture;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			NGUIEditorTools.DrawSprite(tex, new Rect(0f, 0f, 1f, 1f), null);

			// Sprite size label
			Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, "Texture Size: " + tex.width + "x" + tex.height);
		}
	}
}