using UnityEngine;

public class PolygonColliderMirror : MonoBehaviour
{
    [ContextMenu("Mirror X Coordinates")]
    private void MirrorX()
    {
        PolygonCollider2D polygon = GetComponent<PolygonCollider2D>();

        if (polygon == null)
            return;

        for (int path = 0; path < polygon.pathCount; path++)
        {
            Vector2[] points = polygon.GetPath(path);

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2(
                    -points[i].x,
                    points[i].y
                );
            }

            polygon.SetPath(path, points);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(polygon);
#endif
    }
}