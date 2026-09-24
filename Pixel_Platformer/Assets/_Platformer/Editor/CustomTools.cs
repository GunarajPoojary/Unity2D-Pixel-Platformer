using UnityEditor;
using UnityEngine;

public class CustomTools
{
    [MenuItem("TMCustomTools/uGUI/Anchors to Corners %[")]
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

    [MenuItem("TMCustomTools/uGUI/Corners to Anchors %]")]
    static void CornersToAnchors()
    {
        RectTransform t = Selection.activeTransform as RectTransform;

        if (t == null) return;

        t.offsetMin = t.offsetMax = new Vector2(0, 0);
    }

    [MenuItem("Tools/Remove Missing Scripts From Selected")]
    private static void RemoveFromSelected()
    {
        int count = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            // Includes children
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            {
                Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove Missing Scripts");
                count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
        }
        Debug.Log($"Removed {count} missing script(s).");
    }

    [MenuItem("Tools/Remove Missing Scripts In Scene")]
    private static void RemoveInScene()
    {
        int count = 0;
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
            count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }
        Debug.Log($"Removed {count} missing script(s) in scene.");
    }
}