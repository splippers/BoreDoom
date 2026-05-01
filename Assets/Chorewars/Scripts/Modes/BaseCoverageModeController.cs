using UnityEngine;
using Chorewars.AR;
using Chorewars.Core;

namespace Chorewars.Modes
{
    public abstract class BaseCoverageModeController : MonoBehaviour, IChoreMode
    {
        [SerializeField] private string modeId;
        [SerializeField] private string displayName;
        [SerializeField] private string exportPrefix = "session";

        [Header("Mode Wiring")]
        public ChoreDefinition chore;
        public Transform trackedTool;
        public PathTracker pathTracker;
        public CoverageMap coverageMap;
        public SpatialMeshTracker spatialMeshTracker;
        public HouseMapRecorder houseMapRecorder;

        protected ChoreSession Session;

        public string ModeId => modeId;
        public string DisplayName => displayName;

        protected virtual void Reset()
        {
            trackedTool = transform;
        }

        protected virtual void Update()
        {
            if (Session == null) return;
            if (trackedTool == null || coverageMap == null) return;
            coverageMap.MarkVisitedWorldPosition(trackedTool.position);
        }

        public virtual void Begin()
        {
            Session = new ChoreSession
            {
                sessionId = System.Guid.NewGuid().ToString(),
                chore = chore,
                startTimeUtc = System.DateTime.UtcNow
            };

            spatialMeshTracker?.StartScanning();
            houseMapRecorder?.Begin();
        }

        public virtual void End()
        {
            if (Session == null) return;

            Session.endTimeUtc = System.DateTime.UtcNow;
            if (coverageMap != null) Session.coveragePercent = coverageMap.ComputeCoveragePercent();

            var result = ScoringEngine.Score(Session);
            _ = result;

            spatialMeshTracker?.StopScanning();
            if (spatialMeshTracker != null)
            {
                var exportPath = spatialMeshTracker.ExportCurrentSnapshotAsObj(exportPrefix);
                Debug.Log($"[BoreDOOM] Spatial mesh OBJ exported to: {exportPath}");
            }

            houseMapRecorder?.End();
        }
    }
}

