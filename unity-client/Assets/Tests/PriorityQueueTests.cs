using NUnit.Framework;
using NitroRush.AI;

namespace NitroRush.Tests
{
    public class PriorityQueueTests
    {
        [Test]
        public void PushAndPop_MinHeapOrdering()
        {
            var pq = new PriorityQueue<string, float>();
            pq.Push("C", 3.0f);
            pq.Push("A", 1.0f);
            pq.Push("B", 2.0f);

            Assert.AreEqual(3, pq.Count);
            Assert.AreEqual("A", pq.Pop());
            Assert.AreEqual("B", pq.Pop());
            Assert.AreEqual("C", pq.Pop());
            Assert.AreEqual(0, pq.Count);
        }

        [Test]
        public void DuplicatePriorities_MaintainsCount()
        {
            var pq = new PriorityQueue<string, float>();
            pq.Push("First", 5.0f);
            pq.Push("Second", 5.0f);

            Assert.AreEqual(2, pq.Count);
            string p1 = pq.Pop();
            string p2 = pq.Pop();
            Assert.IsTrue(p1 == "First" || p1 == "Second");
            Assert.AreEqual(0, pq.Count);
        }
    }
}
