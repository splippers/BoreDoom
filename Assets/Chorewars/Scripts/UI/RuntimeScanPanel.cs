using UnityEngine;
using Chorewars.AR;

namespace Chorewars.UI
{
    /// <summary>
    /// IMGUI-based runtime panel so the two-room demo can run without building a Canvas in-editor.
    /// Toggle with F1 in-editor; on-device it's always available.
    /// </summary>
    public class RuntimeScanPanel : MonoBehaviour
    {
        [SerializeField] private HouseScanSessionController scanSession;
        [SerializeField] private SpatialMeshTracker meshTracker;
        [SerializeField] private float uiScale = 1.6f;

        private bool _visible = true;

        private void Awake()
        {
            if (scanSession == null) scanSession = FindFirstObjectByType<HouseScanSessionController>();
            if (meshTracker == null) meshTracker = FindFirstObjectByType<SpatialMeshTracker>();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F1)) _visible = !_visible;
#endif
        }

        private void OnGUI()
        {
            if (!_visible) return;

            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * uiScale);

            GUILayout.BeginArea(new Rect(12, 12, 360, 320), GUI.skin.window);
            GUILayout.Label("BoreDOOM – Scan Panel");

            bool scanning = scanSession != null && scanSession.IsScanning;
            GUILayout.Label($"Scanning: {scanning}");
            if (meshTracker != null) GUILayout.Label($"Meshes: {meshTracker.MeshCount}");

            GUILayout.Space(8);

            GUI.enabled = scanSession != null && !scanning;
            if (GUILayout.Button("Start scan (room A → room B)")) scanSession.StartScan();

            GUI.enabled = scanSession != null && scanning;
            if (GUILayout.Button("Stop scan")) scanSession.StopScan();

            GUI.enabled = scanSession != null;
            if (GUILayout.Button("Export combined OBJ")) scanSession.ExportCombinedObj();

            GUI.enabled = meshTracker != null;
            if (GUILayout.Button("Clear meshes")) meshTracker.ClearAllMeshes();

            GUI.enabled = true;
            GUILayout.EndArea();

            GUI.matrix = old;
        }
    }
}

