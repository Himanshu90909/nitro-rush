using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.AI
{
    /// <summary>
    /// Represents a directed edge to a neighboring waypoint node in the AI track graph.
    /// </summary>
    public class WaypointEdge
    {
        public WaypointNode TargetNode { get; set; }
        public float BaseCost { get; set; }

        public float TotalCost => BaseCost + (TargetNode != null ? TargetNode.TotalPenalty : 0f);

        public WaypointEdge(WaypointNode targetNode, float baseCost)
        {
            TargetNode = targetNode;
            BaseCost = baseCost;
        }
    }

    /// <summary>
    /// Track graph node representing a position on or off the track with dynamic penalty support.
    /// </summary>
    public class WaypointNode
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public List<WaypointEdge> Neighbors { get; set; } = new List<WaypointEdge>();
        public bool IsShortcut { get; set; }
        public float TrafficPenalty { get; set; }
        public float OffRoadPenalty { get; set; }

        public float TotalPenalty => TrafficPenalty + OffRoadPenalty;

        public WaypointNode(int id, Vector3 position, bool isShortcut = false)
        {
            Id = id;
            Position = position;
            IsShortcut = isShortcut;
            TrafficPenalty = 0f;
            OffRoadPenalty = 0f;
        }

        public void AddNeighbor(WaypointNode target, float moveCost)
        {
            if (target == null) return;
            Neighbors.Add(new WaypointEdge(target, moveCost));
        }

        public void UpdatePenalties(float traffic, float offRoad)
        {
            TrafficPenalty = Mathf.Max(0f, traffic);
            OffRoadPenalty = Mathf.Max(0f, offRoad);
        }
    }
}
