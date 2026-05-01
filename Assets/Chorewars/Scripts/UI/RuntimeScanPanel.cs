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
        [SerializeField] private HomeOriginAligner homeOriginAligner;
#if CHOREWARS_META_XR
        [SerializeField] private Chorewars.Integration.MetaXrHomeOriginProvider metaHomeOrigin;
#endif
        [SerializeField] private float uiScale = 1.6f;

        private bool _visible = true;

        private void Awake()
        {
            if (scanSession == null) scanSession = FindFirstObjectByType<HouseScanSessionController>();
            if (meshTracker == null) meshTracker = FindFirstObjectByType<SpatialMeshTracker>();
            if (homeOriginAligner == null) homeOriginAligner = FindFirstObjectByType<HomeOriginAligner>();
#if CHOREWARS_META_XR
            if (metaHomeOrigin == null) metaHomeOrigin = FindFirstObjectByType<Chorewars.Integration.MetaXrHomeOriginProvider>();
#endif
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

            GUILayout.BeginArea(new Rect(12, 12, 420, 520), GUI.skin.window);
            GUILayout.Label("BoreDOOM – Scan Panel");

            bool scanning = scanSession != null && scanSession.IsScanning;
            GUILayout.Label($"Scanning: {scanning}");
            if (scanSession != null) GUILayout.Label($"Elapsed: {scanSession.ElapsedSeconds:0.0}s");
            if (scanSession != null) GUILayout.Label($"Coverage: {scanSession.CoveragePercent:0.0}%");
            if (meshTracker != null) GUILayout.Label($"Meshes: {meshTracker.MeshCount}");

#if CHOREWARS_META_XR
            GUILayout.Space(6);
            GUILayout.Label("Home origin (Meta XR)");
            if (metaHomeOrigin != null)
            {
                GUILayout.Label($"Supported: {metaHomeOrigin.IsSupported}");
                GUILayout.Label($"StoredUuid: {metaHomeOrigin.HasStoredUuid}");
                GUILayout.Label($"Localized: {metaHomeOrigin.IsLocalized}");
                if (metaHomeOrigin.Uuid != System.Guid.Empty)
                    GUILayout.Label($"Uuid: {metaHomeOrigin.Uuid:D}");
            }
            else
            {
                GUILayout.Label("MetaXrHomeOriginProvider: not found in scene");
            }
#else
            GUILayout.Space(6);
            GUILayout.Label("Home origin: compile with CHOREWARS_META_XR + Meta XR SDK");
#endif

            GUILayout.Space(8);

            GUI.enabled = homeOriginAligner != null;
            if (GUILayout.Button("Set Home Origin (uses current ScanRoot pose)")) homeOriginAligner.SetHomeOriginToCurrent();
            if (GUILayout.Button("Apply Home Origin (retry localize)")) homeOriginAligner.ApplyIfAvailable();
            GUI.enabled = true;

            GUILayout.Space(8);

            GUI.enabled = scanSession != null && !scanning;
            if (GUILayout.Button("Start scan (room A → room B)")) scanSession.StartScan();

            GUI.enabled = scanSession != null && scanning;
            if (GUILayout.Button("Stop scan")) scanSession.StopScan();

            GUI.enabled = scanSession != null;
            if (GUILayout.Button("Export combined OBJ")) scanSession.ExportCombinedObj();

            GUI.enabled = scanSession != null;
            if (GUILayout.Button("Take snapshot (house-map)")) scanSession.TakeSnapshot();

            GUI.enabled = meshTracker != null;
            if (GUILayout.Button("Clear meshes")) meshTracker.ClearAllMeshes();

            GUI.enabled = true;
            GUILayout.EndArea();

            GUI.matrix = old;
        }
    }
}

