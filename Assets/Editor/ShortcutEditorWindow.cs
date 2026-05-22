using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ShortcutEditorWindow : EditorWindow
{
	[MenuItem("Window/EditorShortcut", false, 9999)]
	public static void Init()
	{
		var window = (ShortcutEditorWindow)GetWindow(typeof(ShortcutEditorWindow));
		window.Show();
	}
	
	private void OnGUI()
	{
		if (GUILayout.Button("Trace Tracks"))
		{
			TrackManager.TraceTracks();
		}

		if (GUILayout.Button("Position Entities"))
		{
			EntityManager.PositionEntities();
		}

		if (GUILayout.Button("Clear Progress"))
		{
			PlayerPrefs.DeleteAll();
		}
	}
}
