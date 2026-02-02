using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    List<Node> currentPath;

    public void DrawPath(List<Node> path) => currentPath = path;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (currentPath == null) return;
        foreach (Node n in currentPath)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(n.worldPos, Vector3.one * 0.5f);
        }
    }
#endif
}
