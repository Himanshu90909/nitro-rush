using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.Racing
{
    [Serializable]
    public struct ShortcutLink
    {
        public int EntranceCheckpointIndex;
        public int ExitCheckpointIndex;
        public float DistanceSavedMeters;
    }

    /// <summary>
    /// ScriptableObject defining track route layout, waypoint coordinates, total lap count,
    /// Addressable scene key, and target par times across difficulty levels.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrackData", menuName = "NitroRush/Track Data")]
    public class TrackData : ScriptableObject
    {
        [Header("Track Metadata")]
        public string trackId = "track_neon_city";
        public string displayName = "Neon City Circuit";
        public string sceneAddress = "Assets/Scenes/Tracks/NeonCity.unity";
        public int totalLaps = 3;

        [Header("Track Geometry")]
        public List<Vector3> waypoints = new List<Vector3>();
        public List<int> checkpointIndices = new List<int>();
        public List<ShortcutLink> shortcuts = new List<ShortcutLink>();

        [Header("Par Times (Seconds)")]
        public float easyParTimeSeconds = 180.0f;
        public float mediumParTimeSeconds = 150.0f;
        public float hardParTimeSeconds = 120.0f;
    }
}
