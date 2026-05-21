using UnityEngine;
using Chorewars.AR;
using Chorewars.Core;
using Chorewars.UI;

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

        [Header("UI")]
        [SerializeField] private SessionSummaryUI summaryUI;
        [SerializeField] private HUDController hud;

        protected ChoreSession Session;
        public ChoreResult LastResult { get; private set; }

        public string ModeId => modeId;
        public string DisplayName => displayName;

        // ── Virtual hooks for subclasses ─────────────────────────────────────
        protected virtual void OnModeBegun() { }
        protected virtual void OnModeEnded(ChoreResult result) { }
        // Called every Update() frame when a new position is sampled by the tool
        protected virtual void OnTrackedPositionSampled(Vector3 worldPos) { }
        // Subclasses return bonus points added on top of the base coverage score
        protected virtual float GetBonusPoints() => 0f;

        protected virtual void Reset() => trackedTool = transform;

        protected virtual void Update()
        {
            if (Session == null) return;
            if (trackedTool == null || coverageMap == null) return;

            var pos = trackedTool.position;
            coverageMap.MarkVisitedWorldPosition(pos);
            OnTrackedPositionSampled(pos);

            hud?.SetCoverage(coverageMap.ComputeCoveragePercent());
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
            OnModeBegun();
        }

        public virtual void End()
        {
            if (Session == null) return;

            Session.endTimeUtc = System.DateTime.UtcNow;
            if (coverageMap != null) Session.coveragePercent = coverageMap.ComputeCoveragePercent();

            var result = ScoringEngine.Score(Session);
            result.totalPoints += (int)GetBonusPoints();
            // Re-grade after bonus
            result.grade = result.totalPoints switch
            {
                >= 1100 => "S+",
                >= 900  => "S",
                >= 750  => "A",
                >= 600  => "B",
                >= 400  => "C",
                _       => "D"
            };
            LastResult = result;

            spatialMeshTracker?.StopScanning();
            if (spatialMeshTracker != null)
            {
                var exportPath = spatialMeshTracker.ExportCurrentSnapshotAsObj(exportPrefix);
                Debug.Log($"[BoreDOOM] Mesh exported: {exportPath}");
            }
            houseMapRecorder?.End();

            OnModeEnded(result);
            summaryUI?.Show(result);
        }
    }
}
