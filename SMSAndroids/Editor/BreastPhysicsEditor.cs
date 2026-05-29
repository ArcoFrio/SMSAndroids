using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BreastPhysics))]
public class BreastPhysicsEditor : Editor
{
    private const float HandleSize = 0.06f;
    private const float PickSize   = 0.08f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox(
            "Drag the magenta handles in the Scene view to reshape the pin curve.",
            MessageType.Info);
    }

    private void OnSceneGUI()
    {
        BreastPhysics bp = (BreastPhysics)target;
        if (bp.pinPoints == null || bp.pinPoints.Length == 0) return;

        Transform t = bp.transform;
        bool changed = false;

        Handles.color = Color.magenta;

        for (int i = 0; i < bp.pinPoints.Length; i++)
        {
            // Convert local → world for the handle
            Vector3 worldPos = t.TransformPoint(bp.pinPoints[i].x, bp.pinPoints[i].y, 0f);

            // Draw connecting lines between consecutive points
            if (i > 0)
            {
                Vector3 prev = t.TransformPoint(bp.pinPoints[i - 1].x, bp.pinPoints[i - 1].y, 0f);
                Handles.DrawLine(prev, worldPos);
            }

            // Draggable handle
            float size = HandleUtility.GetHandleSize(worldPos) * HandleSize;
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.FreeMoveHandle(
                worldPos,
                size,
                Vector3.zero,
                Handles.SphereHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bp, "Move Pin Point");
                // Convert world → local and keep Z = 0
                Vector2 local = t.InverseTransformPoint(newWorldPos);
                bp.pinPoints[i] = local;
                changed = true;
            }

            // Label each point for clarity
            Handles.Label(worldPos + Vector3.right * size * 1.8f,
                          $"P{i}",
                          EditorStyles.miniLabel);
        }

        if (changed)
            EditorUtility.SetDirty(bp);
    }
}
