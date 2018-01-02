using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuiltedInSceneView : EditorWindow
{
	private Vector2 _scrollPos;

	[MenuItem("Window/Build in scene view")]
	private static void InitWindow()
	{
		BuiltedInSceneView window = GetWindow<BuiltedInSceneView>(false, "Scenes");
		window.position =  new Rect(window.position.width/2.0f, window.position.height/2.0f, 200.0f, 400.0f);
	}


	private void OnGUI()
	{
		EditorGUILayout.BeginVertical();

		GUILayout.Label("Builted Scenes", EditorStyles.largeLabel);
		int inx = 1;

		foreach (var scene in EditorBuildSettings.scenes)
		{
			if (scene.enabled)
			{
				bool isPresed = GUILayout.Button(string.Format("{0} - {1}",inx++, Path.GetFileNameWithoutExtension(scene.path)), new GUIStyle(GUI.skin.button){alignment = TextAnchor.UpperLeft});
				if(isPresed)
					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
						EditorSceneManager.OpenScene(scene.path);
			}
		}

		EditorGUILayout.EndVertical();
	}
}