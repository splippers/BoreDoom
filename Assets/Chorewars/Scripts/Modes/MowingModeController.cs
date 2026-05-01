using UnityEngine;
using Chorewars.AR;
using Chorewars.Core;

namespace Chorewars.Modes
{
    public class MowingModeController : MonoBehaviour, IChoreMode
    {
        [SerializeField] private string modeId = "mowing";
        [SerializeField] private string displayName = "Mowing";

        [Header("Mode Wiring")]
        public ChoreDefinition mowingChore;
        public Transform trackedTool;
        public PathTracker pathTracker;
        public CoverageMap coverageMap;
        public SpatialMeshTracker spatialMeshTracker;

        private ChoreSession _session;

        public string ModeId => modeId;
        public string DisplayName => displayName;

        private void Reset()
        {
            trackedTool = transform;
        }

        private void Update()
        {
            if (_session == null) return;
            if (trackedTool == null || coverageMap == null) return;
            coverageMap.MarkVisitedWorldPosition(trackedTool.position);
        }

        public void Begin()
        {
            _session = new ChoreSession
            {
                sessionId = System.Guid.NewGuid().ToString(),
                chore = mowingChore,
                startTimeUtc = System.DateTime.UtcNow
            };

            spatialMeshTracker?.StartScanning();
        }

        public void End()
        {
            if (_session == null) return;

            _session.endTimeUtc = System.DateTime.UtcNow;
            if (coverageMap != null) _session.coveragePercent = coverageMap.ComputeCoveragePercent();

            var result = ScoringEngine.Score(_session);
            _ = result;

            spatialMeshTracker?.StopScanning();
            if (spatialMeshTracker != null)
            {
                var exportPath = spatialMeshTracker.ExportCurrentSnapshotAsObj("lawn-mow");
                Debug.Log($"[BoreDOOM] Spatial mesh OBJ exported to: {exportPath}");
            }
        }
    }
}
