using System.Collections.Generic;
using UnityEngine;
using Chorewars.Modes;
using Chorewars.AR;

namespace Chorewars.Modes
{
    /// <summary>
    /// Perfect Grid mode: hoover your room in perfectly parallel stripes like a football pitch.
    /// AR renders target stripe lines; alignment to the grid earns bonus points.
    ///
    /// Inherits coverage tracking from BaseCoverageModeController.
    /// Adds an alignment score: how closely did the user follow the stripe pattern?
    /// </summary>
    public class PerfectGridModeController : BaseCoverageModeController
    {
        [Header("Grid settings")]
        [SerializeField] private float stripeWidthMeters = 0.5f;
        [SerializeField] private GridPattern pattern = GridPattern.Parallel;
        [SerializeField] private GameObject stripeLinePrefab;  // LineRenderer prefab per stripe
        [SerializeField] private Material stripeIdleMat;
        [SerializeField] private Material stripeClearedMat;

        public enum GridPattern { Parallel, Diagonal, Herringbone, Spiral }

        private readonly List<StripeZone> _stripes = new();
        private float _alignmentScore;

        [System.Serializable]
        private class StripeZone
        {
            public float worldZ;          // Z-coordinate of stripe centre in room space
            public float coveragePercent; // 0-100
            public GameObject lineObj;
        }

        protected override void OnModeBegun()
        {
            base.OnModeBegun();
            GenerateStripes();
        }

        private void GenerateStripes()
        {
            _stripes.Clear();
            // Divide the room bounds into stripes along Z axis
            var bounds = GetRoomBounds(); // from BaseCoverageModeController or CoverageMap
            int count = Mathf.CeilToInt((bounds.size.z) / stripeWidthMeters);
            for (int i = 0; i < count; i++)
            {
                float z = bounds.min.z + (i + 0.5f) * stripeWidthMeters;
                var stripe = new StripeZone { worldZ = z };
                if (stripeLinePrefab != null)
                    stripe.lineObj = Instantiate(stripeLinePrefab);
                _stripes.Add(stripe);
            }
        }

        protected override void OnTrackedPositionSampled(Vector3 worldPos)
        {
            base.OnTrackedPositionSampled(worldPos);

            // Find nearest stripe
            float minDist = float.MaxValue;
            StripeZone nearest = null;
            foreach (var s in _stripes)
            {
                float d = Mathf.Abs(worldPos.z - s.worldZ);
                if (d < minDist) { minDist = d; nearest = s; }
            }

            if (nearest == null) return;

            // Alignment bonus: within half a stripe width = following the grid
            if (minDist < stripeWidthMeters * 0.5f)
            {
                nearest.coveragePercent = Mathf.Min(100f, nearest.coveragePercent + 2f);
                if (nearest.lineObj != null && nearest.coveragePercent >= 80f)
                    nearest.lineObj.GetComponent<Renderer>()?.sharedMaterial.SetFloat("_Cleared", 1f);
            }

            // Recompute alignment score = average of per-stripe coverage
            float total = 0f;
            foreach (var s in _stripes) total += s.coveragePercent;
            _alignmentScore = _stripes.Count > 0 ? total / _stripes.Count : 0f;
        }

        protected override float GetBonusPoints()
        {
            // Up to 300 bonus points for perfect grid alignment
            return (_alignmentScore / 100f) * 300f;
        }

        // Stub — override to pull from your CoverageMap or room data
        private Bounds GetRoomBounds() => new Bounds(Vector3.zero, new Vector3(5f, 3f, 5f));
    }
}
