﻿using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Panel wizard that allows a bird's eye view of all cameras in your scene.
/// </summary>

public class UICameraTool : EditorWindow
{
	/// <summary>
	/// Layer mask field, originally from:
	/// http://answers.unity3d.com/questions/60959/mask-field-in-the-editor.html
	/// </summary>

	public static int LayerMaskField (string label, int mask, params GUILayoutOption[] options)
	{
		List<string> layers = new List<string>();
		List<int> layerNumbers = new List<int>();

		string selectedLayers = "";

		for (int i = 0; i < 32; ++i)
		{
			string layerName = LayerMask.LayerToName(i);

			if (!string.IsNullOrEmpty(layerName))
			{
				if (mask == (mask | (1 << i)))
				{
					if (string.IsNullOrEmpty(selectedLayers))
					{
						selectedLayers = layerName;
					}
					else
					{
						selectedLayers = "Mixed";
					}
				}
			}
		}

		if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.ExecuteCommand)
		{
			if (mask == 0)
			{
				layers.Add("Nothing");
			}
			else if (mask == -1)
			{
				layers.Add("Everything");
			}
			else
			{
				layers.Add(selectedLayers);
			}
			layerNumbers.Add(-1);
		}

		layers.Add((mask == 0 ? "[+] " : "      ") + "Nothing");
		layerNumbers.Add(-2);

		layers.Add((mask == -1 ? "[+] " : "      ") + "Everything");
		layerNumbers.Add(-3);

		for (int i = 0; i < 32; ++i)
		{
			string layerName = LayerMask.LayerToName(i);

			if (layerName != "")
			{
				if (mask == (mask | (1 << i)))
				{
					layers.Add("[+] " + layerName);
				}
				else
				{
					layers.Add("      " + layerName);
				}
				layerNumbers.Add(i);
			}
		}

		bool preChange = GUI.changed;

		GUI.changed = false;

		int newSelected = 0;

		if (Event.current.type == EventType.MouseDown)
		{
			newSelected = -1;
		}

		if (string.IsNullOrEmpty(label))
		{
			newSelected = EditorGUILayout.Popup(newSelected, layers.ToArray(), EditorStyles.layerMaskField, options);
		}
		else
		{
			newSelected = EditorGUILayout.Popup(label, newSelected, layers.ToArray(), EditorStyles.layerMaskField, options);
		}

		if (GUI.changed && newSelected >= 0)
		{
			if (newSelected == 0)
			{
				mask = 0;
			}
			else if (newSelected == 1)
			{
				mask = -1;
			}
			else
			{
				if (mask == (mask | (1 << layerNumbers[newSelected])))
				{
					mask &= ~(1 << layerNumbers[newSelected]);
				}
				else
				{
					mask = mask | (1 << layerNumbers[newSelected]);
				}
			}
		}
		else
		{
			GUI.changed = preChange;
		}
		return mask;
	}

	public static int LayerMaskField (int mask, params GUILayoutOption[] options)
	{
		return LayerMaskField(null, mask, options);
	}

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);

		Camera[] cams = Resources.FindObjectsOfTypeAll(typeof(Camera)) as Camera[];
		List<Camera> list = new List<Camera>();
		
		foreach (Camera c in cams)
		{
			if (c.name != "SceneCamera" && c.name != "Preview Camera")
			{
#if UNITY_3_4
				PrefabType type = EditorUtility.GetPrefabType(c.gameObject);
#else
				PrefabType type = PrefabUtility.GetPrefabType(c.gameObject);
#endif
				if (type != PrefabType.Prefab) list.Add(c);
			}
		}

		if (list.Count > 0)
		{
			DrawRow(null);
			NGUIEditorTools.DrawSeparator();
			foreach (Camera cam in list) DrawRow(cam);
		}
		else
		{
			GUILayout.Label("No cameras found in the scene");
		}
	}

	/// <summary>
	/// Helper function used to print things in columns.
	/// </summary>

	void DrawRow (Camera cam)
	{
		GUILayout.BeginHorizontal();
		{
			bool enabled = (cam == null || (cam.gameObject.active && cam.enabled));

			GUI.color = Color.white;

			if (cam != null)
			{
				if (enabled != EditorGUILayout.Toggle(enabled, GUILayout.Width(20f)))
				{
					cam.enabled = !enabled;
					EditorUtility.SetDirty(cam.gameObject);
				}
			}
			else
			{
				GUILayout.Space(30f);
			}

			bool highlight = (cam == null || Selection.activeGameObject == null) ? false :
				(0 != (cam.cullingMask & (1 << Selection.activeGameObject.layer)));

			if (enabled)
			{
				GUI.color = highlight ? new Color(0f, 0.8f, 1f) : Color.white;
			}
			else
			{
				GUI.color = highlight ? new Color(0f, 0.5f, 0.8f) : Color.grey;
			}

			GUILayout.Label(cam == null ? "Camera's Name" : cam.name + (cam.orthographic ? " (2D)" : " (3D)"), GUILayout.MinWidth(100f));
			GUILayout.Label(cam == null ? "Layer" : LayerMask.LayerToName(cam.gameObject.layer), GUILayout.Width(70f));

			GUI.color = enabled ? Color.white : new Color(0.7f, 0.7f, 0.7f);

			if (cam == null)
			{
				GUILayout.Label("EV", GUILayout.Width(20f));
			}
			else
			{
				UICamera uic = cam.GetComponent<UICamera>();
				bool ev = (uic != null && uic.enabled);

				if (ev != EditorGUILayout.Toggle(ev, GUILayout.Width(20f)))
				{
					if (uic == null) uic = cam.gameObject.AddComponent<UICamera>();
					uic.enabled = !ev;
				}
			}

			if (cam == null)
			{
				GUILayout.Label("Mask", GUILayout.Width(100f));
			}
			else
			{
				int mask = LayerMaskField(cam.cullingMask, GUILayout.Width(105f));

				if (cam.cullingMask != mask)
				{
					Undo.RegisterUndo(cam, "Camera Mask Change");
					cam.cullingMask = mask;
					EditorUtility.SetDirty(cam.gameObject);
				}
			}

			if (cam)
			{
				if (GUILayout.Button("Select", GUILayout.Width(50f)))
				{
					Selection.activeGameObject = cam.gameObject;
					EditorUtility.SetDirty(cam.gameObject);
				}
			}
			else
			{
				GUILayout.Space(60f);
			}
		}
		GUILayout.EndHorizontal();
	}
}