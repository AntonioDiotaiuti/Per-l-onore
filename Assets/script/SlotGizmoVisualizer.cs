
using UnityEngine;

public class SlotGizmoVisualizer : MonoBehaviour
{
    public Color gizmoColor = Color.green;
    public Vector3 boxSize = new Vector3(0.2f, 0.01f, 0.2f);

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
