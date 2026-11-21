using UnityEngine;

public class BoxGizmo : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private bool drawSolid = false;
    [SerializeField] private bool onlyWhenSelected = false;

    private void OnDrawGizmos()
    {
        if (!onlyWhenSelected)
        {
            DrawBox();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (onlyWhenSelected)
        {
            DrawBox();
        }
    }

    private void DrawBox()
    {
        // Apply the full transform (position, rotation, scale) to the gizmo
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;

        if (drawSolid)
        {
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
        else
        {
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        // Restore previous gizmo state
        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }
#endif
}
