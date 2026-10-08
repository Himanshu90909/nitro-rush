using NUnit.Framework;
using UnityEngine;

namespace NitroRush.Tests
{
    public class InterpolationTests
    {
        [Test]
        public void SnapshotInterpolation_MidpointLerp()
        {
            Vector3 startPos = new Vector3(0, 0, 0);
            Vector3 endPos = new Vector3(0, 0, 100);

            float startTime = 0f;
            float endTime = 1.0f;
            float renderTime = 0.5f;

            float t = Mathf.Clamp01((renderTime - startTime) / (endTime - startTime));
            Vector3 result = Vector3.Lerp(startPos, endPos, t);

            Assert.AreEqual(0.5f, t, 0.001f);
            Assert.AreEqual(new Vector3(0, 0, 50), result);
        }

        [Test]
        public void SnapshotInterpolation_ClampedOutRange()
        {
            Vector3 startPos = new Vector3(0, 0, 0);
            Vector3 endPos = new Vector3(0, 0, 100);

            float tOver = Mathf.Clamp01((2.0f - 0f) / 1.0f);
            Vector3 result = Vector3.Lerp(startPos, endPos, tOver);

            Assert.AreEqual(1.0f, tOver, 0.001f);
            Assert.AreEqual(endPos, result);
        }
    }
}
