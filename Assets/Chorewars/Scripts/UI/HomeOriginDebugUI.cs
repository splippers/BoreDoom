using UnityEngine;
using Chorewars.AR;

namespace Chorewars.UI
{
    /// <summary>
    /// Minimal runtime UI hook for setting/applying home origin + exporting scans.
    /// Intended as a Quest-on-device debug panel (wire buttons in Unity).
    /// </summary>
    public class HomeOriginDebugUI : MonoBehaviour
    {
        [SerializeField] private HomeOriginAligner homeOriginAligner;
        [SerializeField] private SpatialMeshTracker spatialMeshTracker;
        [SerializeField] private string exportPrefix = "combined-house";

        public void ApplyHomeOrigin()
        {
            if (homeOriginAligner == null) return;
            bool ok = homeOriginAligner.ApplyIfAvailable();
            Debug.Log($"[BoreDOOM] ApplyHomeOrigin ok={ok}");
        }

        public void SetHomeOriginToCurrent()
        {
            if (homeOriginAligner == null) return;
            homeOriginAligner.SetHomeOriginToCurrent();
            Debug.Log("[BoreDOOM] SetHomeOriginToCurrent invoked");
        }

        public void ExportCombinedObj()
        {
            if (spatialMeshTracker == null) return;
            var path = spatialMeshTracker.ExportCurrentSnapshotAsObj(exportPrefix);
            Debug.Log($"[BoreDOOM] ExportCombinedObj path={path}");
        }
    }
}

