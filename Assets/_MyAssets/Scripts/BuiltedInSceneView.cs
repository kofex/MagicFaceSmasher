using UnityEditor;
using UnityEngine;

public class BuiltedInSceneView : EditorWindow
{
	private Vector2 _scrollPos;

	[MenuItem("Window/Scene View")]
	private static void InitWindow()
	{
		//EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
		BuiltedInSceneView window = GetWindow<BuiltedInSceneView>(false, "Scenes in Builds");
		window.position =  new Rect(window.position.width/2.0f, window.position.height/2.0f, 200.0f, 400.0f);
	}


	private void OnGUI()
	{
		EditorGUILayout.BeginVertical();

		GUILayout.Label("Builted Scenes", EditorStyles.largeLabel);
		foreach (var scene in EditorBuildSettings.scenes)
		{
			int inx = 1;
			if (scene.enabled)
			{
				GUILayout.Button(string.Format("{0} - {1}",inx, scene), new GUIStyle(GUI.skin.button){alignment = TextAnchor.MiddleCenter});
				inx++;
			}
		}

		EditorGUILayout.EndVertical();
	}
}