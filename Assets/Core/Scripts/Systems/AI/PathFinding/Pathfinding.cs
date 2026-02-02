using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Transform seeker, target;
    GridSystem grid;

    void Awake() => grid = GetComponent<GridSystem>();
    void Update() => FindPath(seeker.position, target.position);

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
                if (openSet[i].fCost < current.fCost || openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost)
                    current = openSet[i];

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(current))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

                int newCostToNeighbour = current.gCost + GetDistance(current, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = current;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node current = endNode;

        // Safety: prevent infinite loop when parent chain is broken
        int safetyCounter = 0;
        int maxSteps = 10;
        if (grid != null && grid.grid != null)
            maxSteps = grid.grid.GetLength(0) * grid.grid.GetLength(1) + 10; // a reasonable upper bound

        while (current != null && current != startNode && safetyCounter < maxSteps)
        {
            path.Add(current);
            current = current.parent;
            safetyCounter++;
        }

        if (safetyCounter >= maxSteps)
        {
            Debug.LogWarning("RetracePath aborted: exceeded maximum steps. Path may be invalid or contain a loop.");
        }

        // If we exited because current is null, warn and return an empty path
        if (current == null)
        {
            Debug.LogWarning("RetracePath aborted: parent chain terminated before reaching startNode.");
            GetComponent<PathVisualizer>().DrawPath(new List<Node>());
            return;
        }

        path.Reverse();

        GetComponent<PathVisualizer>().DrawPath(path);
    }

    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        return 14 * Mathf.Min(dstX, dstY) + 10 * Mathf.Abs(dstX - dstY);
    }
}
