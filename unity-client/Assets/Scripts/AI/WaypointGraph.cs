using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.AI
{
    /// <summary>
    /// Graph data structure holding track waypoints and adjacency details for pathfinding.
    /// </summary>
    public class WaypointGraph
    {
        private readonly List<WaypointNode> _nodes = new List<WaypointNode>();

        public IReadOnlyList<WaypointNode> Nodes => _nodes;

        public void AddNode(WaypointNode node)
        {
            if (node != null && !_nodes.Contains(node))
            {
                _nodes.Add(node);
            }
        }

        public void ConnectNodes(WaypointNode from, WaypointNode to, float moveCost, bool bidirectional = false)
        {
            if (from == null || to == null) return;
            from.AddNeighbor(to, moveCost);
            if (bidirectional)
            {
                to.AddNeighbor(from, moveCost);
            }
        }

        public IEnumerable<WaypointEdge> GetNeighbors(WaypointNode node)
        {
            return node != null ? node.Neighbors : new List<WaypointEdge>();
        }

        public float Heuristic(WaypointNode a, WaypointNode b)
        {
            if (a == null || b == null) return 0f;
            return Vector3.Distance(a.Position, b.Position);
        }

        public WaypointNode GetNearestNode(Vector3 position)
        {
            WaypointNode nearest = null;
            float minDistanceSq = float.MaxValue;

            for (int i = 0; i < _nodes.Count; i++)
            {
                float distSq = (_nodes[i].Position - position).sqrMagnitude;
                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    nearest = _nodes[i];
                }
            }

            return nearest;
        }

        public void BuildFromWaypoints(Vector3[] waypoints, float defaultCost = 1f)
        {
            _nodes.Clear();
            if (waypoints == null || waypoints.Length == 0) return;

            WaypointNode[] createdNodes = new WaypointNode[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                createdNodes[i] = new WaypointNode(i, waypoints[i]);
                AddNode(createdNodes[i]);
            }

            for (int i = 0; i < createdNodes.Length; i++)
            {
                int nextIndex = (i + 1) % createdNodes.Length;
                float distance = Vector3.Distance(createdNodes[i].Position, createdNodes[nextIndex].Position);
                ConnectNodes(createdNodes[i], createdNodes[nextIndex], distance * defaultCost, false);
            }
        }
    }
}
