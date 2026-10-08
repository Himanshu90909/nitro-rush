using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.AI
{
    /// <summary>
    /// Pure C# EditMode-testable A* Pathfinder operating on WaypointGraph nodes.
    /// Calculates optimal route taking into account distance, traffic penalties, and off-road penalties.
    /// </summary>
    public static class AStarPathfinder
    {
        public static List<WaypointNode> FindPath(WaypointNode start, WaypointNode goal)
        {
            if (start == null || goal == null) return new List<WaypointNode>();

            var openSet = new PriorityQueue<WaypointNode, float>();
            var closedSet = new HashSet<WaypointNode>();

            var gScore = new Dictionary<WaypointNode, float>();
            var cameFrom = new Dictionary<WaypointNode, WaypointNode>();

            gScore[start] = 0f;
            openSet.Push(start, Heuristic(start, goal));

            while (openSet.Count > 0)
            {
                WaypointNode current = openSet.Pop();

                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }

                closedSet.Add(current);

                foreach (var edge in current.Neighbors)
                {
                    WaypointNode neighbor = edge.TargetNode;
                    if (neighbor == null || closedSet.Contains(neighbor))
                        continue;

                    // g = distance traveled + move cost + traffic penalty + off-road penalty
                    float stepCost = edge.BaseCost + neighbor.TrafficPenalty + neighbor.OffRoadPenalty;
                    float tentativeGScore = gScore[current] + stepCost;

                    if (!gScore.TryGetValue(neighbor, out float currentG) || tentativeGScore < currentG)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        float fScore = tentativeGScore + Heuristic(neighbor, goal);
                        openSet.Push(neighbor, fScore);
                    }
                }
            }

            return new List<WaypointNode>(); // Unreachable
        }

        public static float PathLength(List<WaypointNode> path)
        {
            if (path == null || path.Count < 2) return 0f;

            float length = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                length += Vector3.Distance(path[i].Position, path[i + 1].Position);
            }
            return length;
        }

        private static float Heuristic(WaypointNode a, WaypointNode b)
        {
            return Vector3.Distance(a.Position, b.Position);
        }

        private static List<WaypointNode> ReconstructPath(Dictionary<WaypointNode, WaypointNode> cameFrom, WaypointNode current)
        {
            var path = new List<WaypointNode> { current };
            while (cameFrom.TryGetValue(current, out WaypointNode parent))
            {
                current = parent;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}
