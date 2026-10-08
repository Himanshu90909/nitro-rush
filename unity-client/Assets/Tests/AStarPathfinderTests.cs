using System.Collections.Generic;
using NUnit.Framework;
using NitroRush.AI;
using UnityEngine;

namespace NitroRush.Tests
{
    public class AStarPathfinderTests
    {
        [Test]
        public void FindPath_ShortestPathFound()
        {
            var graph = new WaypointGraph();
            var n1 = new WaypointNode(1, new Vector3(0, 0, 0));
            var n2 = new WaypointNode(2, new Vector3(0, 0, 10));
            var n3 = new WaypointNode(3, new Vector3(0, 0, 20));

            graph.AddNode(n1);
            graph.AddNode(n2);
            graph.AddNode(n3);

            graph.ConnectNodes(n1, n2, 10f);
            graph.ConnectNodes(n2, n3, 10f);

            List<WaypointNode> path = AStarPathfinder.FindPath(n1, n3);

            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(n1, path[0]);
            Assert.AreEqual(n2, path[1]);
            Assert.AreEqual(n3, path[2]);
        }

        [Test]
        public void FindPath_TrafficPenaltyChangesRoute()
        {
            var n1 = new WaypointNode(1, new Vector3(0, 0, 0));
            var n2Direct = new WaypointNode(2, new Vector3(0, 0, 10));
            var n3Detour = new WaypointNode(3, new Vector3(10, 0, 5));
            var goal = new WaypointNode(4, new Vector3(0, 0, 20));

            // Direct route has high traffic penalty
            n2Direct.TrafficPenalty = 100f;

            n1.AddNeighbor(n2Direct, 10f);
            n2Direct.AddNeighbor(goal, 10f);

            n1.AddNeighbor(n3Detour, 7f);
            n3Detour.AddNeighbor(goal, 7f);

            List<WaypointNode> path = AStarPathfinder.FindPath(n1, goal);

            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(n3Detour, path[1]); // Prefers detour over traffic-clogged direct node
        }

        [Test]
        public void FindPath_UnreachableGoalHandled()
        {
            var n1 = new WaypointNode(1, new Vector3(0, 0, 0));
            var n2Unreachable = new WaypointNode(2, new Vector3(100, 0, 100));

            List<WaypointNode> path = AStarPathfinder.FindPath(n1, n2Unreachable);

            Assert.IsEmpty(path);
        }
    }
}
