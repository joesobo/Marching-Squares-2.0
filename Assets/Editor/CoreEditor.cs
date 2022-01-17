using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class CoreEditor : EditorWindow {
    private CoreScriptableObject core;

    [MenuItem("Window/CORE")]
    public static void ShowWindow() {
        GetWindow(typeof(CoreEditor));
    }

    private void OnGUI() {
        GUILayout.Label("Core Editor", EditorStyles.boldLabel);
        core = (CoreScriptableObject)EditorGUILayout.ObjectField("ScriptableObject", core, typeof(CoreScriptableObject), true);
        if (!core) return;

        GUILayout.Space(20);
        core.seed = EditorGUILayout.IntField("Seed", core.seed);

        using (new EditorGUILayout.HorizontalScope()) {
            GUILayout.Space(10);
            core.voxelResolution = EditorGUILayout.IntField("Voxel Resolution", core.voxelResolution);
            GUILayout.Space(10);
            core.chunkResolution = EditorGUILayout.IntField("Chunk Resolution", core.chunkResolution);
        }
    }
}
