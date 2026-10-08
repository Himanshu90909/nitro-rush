using System.Text;
using UnityEngine;
using NitroRush.Core;
using NitroRush.Racing;

namespace NitroRush.Performance
{
    /// <summary>
    /// OnGUI real-time performance debug overlay toggled with key F8.
    /// Displays FPS, frame time ms, memory usage, GC delta/frame, pool metrics, and active racers.
    /// </summary>
    public class PerformanceOverlay : MonoBehaviour
    {
        [Header("Overlay Options")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F8;
        [SerializeField] private bool showOnStart = true;

        private bool _isVisible;
        private GUIStyle _boxStyle;
        private GUIStyle _textStyle;

        private void Start()
        {
            _isVisible = showOnStart;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                _isVisible = !_isVisible;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            InitStyles();

            Rect panelRect = new Rect(10, 10, 360, 320);
            GUI.Box(panelRect, "", _boxStyle);

            var sb = new StringBuilder();
            sb.AppendLine("<b>NITRO RUSH DEBUG OVERLAY (F8)</b>");
            sb.AppendLine("------------------------------------");

            if (PerformanceMonitor.Instance != null)
            {
                var pm = PerformanceMonitor.Instance;
                float memMb = pm.TotalMemoryBytes / (1024f * 1024f);
                float gcKb = pm.GCAllocDeltaBytes / 1024f;
                sb.AppendLine($"<b>FPS:</b> {pm.FPS:F1} | <b>Frame:</b> {pm.FrameTimeMs:F2} ms");
                sb.AppendLine($"<b>Heap Memory:</b> {memMb:F2} MB");
                sb.AppendLine($"<b>GC Alloc Delta:</b> {gcKb:F2} KB/frame");
            }
            else
            {
                sb.AppendLine("PerformanceMonitor instance not active.");
            }

            sb.AppendLine("------------------------------------");
            sb.AppendLine("<b>POOL MANAGER METRICS</b>");

            if (PoolManager.Instance != null)
            {
                var poolStats = PoolManager.Instance.GetAllStats();
                if (poolStats.Count == 0)
                {
                    sb.AppendLine(" No active object pools registered.");
                }
                else
                {
                    foreach (var stat in poolStats)
                    {
                        sb.AppendLine($" [{stat.PoolKey}] Act:{stat.ActiveCount} Inact:{stat.InactiveCount} HitRate:{stat.HitRate * 100f:F0}%");
                    }
                }
            }
            else
            {
                sb.AppendLine(" PoolManager instance not active.");
            }

            sb.AppendLine("------------------------------------");
            sb.AppendLine("<b>RACE SESSION METRICS</b>");

            if (RaceManager.Instance != null)
            {
                sb.AppendLine($" <b>Race Active:</b> {RaceManager.Instance.IsRaceActive}");
                sb.AppendLine($" <b>Track:</b> {RaceManager.Instance.Track?.displayName ?? "None"}");
            }
            else
            {
                sb.AppendLine(" RaceManager instance not active.");
            }

            GUI.Label(new Rect(20, 15, 340, 310), sb.ToString(), _textStyle);
        }

        private void InitStyles()
        {
            if (_boxStyle == null)
            {
                Texture2D bgTex = new Texture2D(1, 1);
                bgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.85f));
                bgTex.Apply();

                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = bgTex }
                };
            }

            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    richText = true,
                    normal = { textColor = Color.white }
                };
            }
        }
    }
}
