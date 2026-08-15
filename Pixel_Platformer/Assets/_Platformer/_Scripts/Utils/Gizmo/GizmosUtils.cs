using UnityEditor;
using UnityEngine;

public static class GizmosUtils
{
    public static void DrawWireArrow(
        Vector3 origin,
        Vector3 offset,
        Vector3 direction,
        float width,
        float height,
        float lineThickness,
        Color color)
    {
        direction.Normalize();

        var side = Vector3.Cross(direction, Vector3.forward).normalized;

        var center = origin + offset;

        var headHeight = height * 0.35f;
        var shaftHeight = height - headHeight;

        var shaftWidth = width * 0.35f;
        var halfWidth = width * 0.5f;
        var halfShaftWidth = shaftWidth * 0.5f;

        // Assuming arrow pointing upwards on the screen
        var shaftBottom = center;
        var shaftTop = center + direction * shaftHeight;
        var tip = center + direction * height;

        // Shaft corners.
        var bottomLeft = shaftBottom + side * halfShaftWidth;
        var bottomRight = shaftBottom - side * halfShaftWidth;

        var topLeft = shaftTop + side * halfShaftWidth;
        var topRight = shaftTop - side * halfShaftWidth;

        // Head corners.
        var headLeft = shaftTop + side * halfWidth;
        var headRight = shaftTop - side * halfWidth;

        var points = new Vector3[]
        {
            bottomLeft,
            topLeft,
            headLeft,
            tip,
            headRight,
            topRight,
            bottomRight,
            bottomLeft,
        };

        Handles.color = color;
        Handles.DrawAAPolyLine(lineThickness, points);
    }
}