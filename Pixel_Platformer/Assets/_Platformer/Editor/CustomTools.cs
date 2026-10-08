using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CustomTools
{
    [MenuItem("CustomTools/PlayerPref/Delete Player Prefs")]
    static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("CustomTools/uGUI/Anchors to Corners %[")]
    static void AnchorsToCorners()
    {
        RectTransform t = Selection.activeTransform as RectTransform;
        RectTransform pt = Selection.activeTransform.parent as RectTransform;

        if (t == null || pt == null) return;

        Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                            t.anchorMin.y + t.offsetMin.y / pt.rect.height);
        Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                            t.anchorMax.y + t.offsetMax.y / pt.rect.height);

        t.anchorMin = newAnchorsMin;
        t.anchorMax = newAnchorsMax;
        t.offsetMin = t.offsetMax = new Vector2(0, 0);
    }

    [MenuItem("CustomTools/uGUI/Corners to Anchors %]")]
    static void CornersToAnchors()
    {
        RectTransform t = Selection.activeTransform as RectTransform;

        if (t == null) return;

        t.offsetMin = t.offsetMax = new Vector2(0, 0);
    }

    // ---------------------------------------------------------------------
    // Missing script removal
    // ---------------------------------------------------------------------

    /// <summary>Removes missing scripts from root and all children (incl. inactive).</summary>
    private static int RemoveMissingInHierarchy(GameObject root, bool registerUndo)
    {
        int count = 0;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (registerUndo)
                Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove Missing Scripts");
            count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        }
        return count;
    }

    /// <summary>
    /// Opens a prefab asset in isolation, removes missing scripts and saves it back.
    /// Returns the number of removed scripts.
    /// </summary>
    private static int RemoveMissingInPrefabAsset(string assetPath)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(assetPath);
        try
        {
            int count = RemoveMissingInHierarchy(contents, false);
            if (count > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(contents, assetPath);
                Debug.Log($"Saved prefab '{assetPath}' ({count} missing script(s) removed).");
            }
            return count;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    /// <summary>Handles the case where a prefab is open in Prefab Mode.</summary>
    private static int RemoveMissingInPrefabStage(PrefabStage stage)
    {
        int count = RemoveMissingInHierarchy(stage.prefabContentsRoot, true);
        if (count > 0)
        {
            PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.assetPath);
            EditorSceneManager.MarkSceneDirty(stage.scene);
            Debug.Log($"Saved prefab '{stage.assetPath}' ({count} missing script(s) removed).");
        }
        return count;
    }

    [MenuItem("Tools/Remove Missing Scripts From Selected")]
    private static void RemoveFromSelected()
    {
        int count = 0;
        var processedPrefabs = new HashSet<string>();
        var dirtyScenes = new HashSet<UnityEngine.SceneManagement.Scene>();
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

        foreach (GameObject go in Selection.gameObjects)
        {
            if (EditorUtility.IsPersistent(go))
            {
                // Prefab asset selected in the Project window
                string path = AssetDatabase.GetAssetPath(go);
                if (processedPrefabs.Add(path))
                    count += RemoveMissingInPrefabAsset(path);
            }
            else if (stage != null && go.scene == stage.scene)
            {
                // Object inside the prefab currently open in Prefab Mode
                if (processedPrefabs.Add(stage.assetPath))
                    count += RemoveMissingInPrefabStage(stage);
            }
            else
            {
                // Regular scene object
                int removed = RemoveMissingInHierarchy(go, true);
                if (removed > 0) dirtyScenes.Add(go.scene);
                count += removed;
            }
        }

        foreach (var scene in dirtyScenes)
            EditorSceneManager.MarkSceneDirty(scene);

        AssetDatabase.SaveAssets();
        Debug.Log($"Removed {count} missing script(s).");
    }

    [MenuItem("Tools/Remove Missing Scripts In Scene")]
    private static void RemoveInScene()
    {
        int count = 0;

        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            // In Prefab Mode: clean and save the open prefab
            count += RemoveMissingInPrefabStage(stage);
        }
        else
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (removed > 0) EditorSceneManager.MarkSceneDirty(go.scene);
                count += removed;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Removed {count} missing script(s) in scene.");
    }

    [MenuItem("Tools/Remove Missing Scripts From All Prefabs In Project")]
    private static void RemoveFromAllPrefabs()
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.StartsWith("Assets/")) continue; // skip read-only package prefabs

                EditorUtility.DisplayProgressBar("Removing Missing Scripts", path, (float)i / guids.Length);
                count += RemoveMissingInPrefabAsset(path);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Removed {count} missing script(s) from project prefabs.");
    }
}