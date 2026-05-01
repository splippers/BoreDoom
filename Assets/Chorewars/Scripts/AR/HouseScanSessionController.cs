using UnityEngine;

namespace Chorewars.AR
{
    /// <summary>
    /// Thin orchestrator for a single continuous scan session (e.g. walk from room A to room B).
    /// Keeps the "two joined rooms" demo simple: start scan, walk, stop, export.
    /// </summary>
    public class HouseScanSessionController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private SpatialMeshTracker spatialMeshTracker;
        [SerializeField] private HouseMapRecorder houseMapRecorder;
        [SerializeField] private CoverageMap coverageMap;
        [SerializeField] private PathTracker pathTracker;

        [Header("Alignment (optional)")]
        [SerializeField] private HomeOriginAligner homeOriginAligner;

        [Header("Export")]
        [SerializeField] private string exportPrefix = "two-room-scan";

        [Header("State")]
        [SerializeField] private bool autoStart;

        public bool IsScanning { get; private set; }
        public float ElapsedSeconds => IsScanning ? Time.unscaledTime - _startedAt : _lastElapsed;
        public float CoveragePercent => coverageMap != null ? coverageMap.ComputeCoveragePercent() : 0f;

        private float _startedAt;
        private float _lastElapsed;

        private void Awake()
        {
            if (spatialMeshTracker == null) spatialMeshTracker = GetComponentInChildren<SpatialMeshTracker>();
            if (houseMapRecorder == null) houseMapRecorder = GetComponentInChildren<HouseMapRecorder>();
            if (coverageMap == null) coverageMap = FindFirstObjectByType<CoverageMap>();
            if (pathTracker == null) pathTracker = FindFirstObjectByType<PathTracker>();
        }

        private void Start()
        {
            if (autoStart) StartScan();
        }

        public void StartScan()
        {
            if (IsScanning) return;

            // For single-session stitching (two adjacent rooms), Quest tracking provides a single shared frame.
            // If a home origin exists, align to it before starting.
            homeOriginAligner?.ApplyIfAvailable();

            IsScanning = true;
            _startedAt = Time.unscaledTime;
            _lastElapsed = 0f;
            spatialMeshTracker?.ClearAllMeshes();
            spatialMeshTracker?.StartScanning();
            houseMapRecorder?.Begin();

            Debug.Log("[BoreDOOM] Scan started");
        }

        public void StopScan()
        {
            if (!IsScanning) return;

            IsScanning = false;
            _lastElapsed = Time.unscaledTime - _startedAt;
            spatialMeshTracker?.StopScanning();
            houseMapRecorder?.End();

            float coverage = coverageMap != null ? coverageMap.ComputeCoveragePercent() : 0f;
            int points = 0;
            if (pathTracker != null) points = pathTracker.worldPositions.Count;

            Debug.Log($"[BoreDOOM] Scan stopped. coverage%={coverage:0.0} pathPoints={points}");
        }

        public void TakeSnapshot()
        {
            houseMapRecorder?.TakeSnapshot();
        }

        public void ExportCombinedObj()
        {
            if (spatialMeshTracker == null) return;
            var path = spatialMeshTracker.ExportCurrentSnapshotAsObj(exportPrefix);
            Debug.Log($"[BoreDOOM] Combined export: {path}");
        }
    }
}

