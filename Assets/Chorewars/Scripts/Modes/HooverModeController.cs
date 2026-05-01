using UnityEngine;
using Chorewars.AR;
using Chorewars.Core;

namespace Chorewars.Modes
{
    public class HooverModeController : MonoBehaviour, IChoreMode
    {
        [SerializeField] private string modeId = "hoovering";
        [SerializeField] private string displayName = "Hoovering";

        [Header("Mode Wiring")]
        public ChoreDefinition hooverChore;
        public Transform trackedTool;
        public PathTracker pathTracker;
        public CoverageMap coverageMap;
        public SpatialMeshTracker spatialMeshTracker;
        public HouseMapRecorder houseMapRecorder;

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
                chore = hooverChore,
                startTimeUtc = System.DateTime.UtcNow
            };

            spatialMeshTracker?.StartScanning();
            houseMapRecorder?.Begin();
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
                var exportPath = spatialMeshTracker.ExportCurrentSnapshotAsObj("kitchen-vacuum");
                Debug.Log($"[BoreDOOM] Spatial mesh OBJ exported to: {exportPath}");
            }

            houseMapRecorder?.End();
        }
    }
}
