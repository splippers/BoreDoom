using UnityEngine;
using Chorewars.Core;

namespace Chorewars.Modes
{
    /// <summary>
    /// Attic Attack: clean a dark, cluttered attic space.
    ///
    /// Unique mechanics:
    /// - Flashlight radius: coverage only counts within a cone in front of the player
    /// - Cobweb zones: AR cobweb overlays must be "swept" (dwell for 2 seconds)
    /// - Darkness penalty: total score × 0.8 unless you clear 80%+ of cobwebs
    /// - Bonus: ceiling coverage (look up) earns double points
    /// </summary>
    public class AtticAttackModeController : BaseCoverageModeController
    {
        [Header("Attic Attack settings")]
        [SerializeField] private float flashlightConeAngle = 60f;
        [SerializeField] private int cobwebCount = 8;
        [SerializeField] private float cobwebDwellSeconds = 2f;
        [SerializeField] private GameObject cobwebPrefab;
        [SerializeField] private GameObject flashlightVfxPrefab;

        private readonly CobwebZone[] _cobwebs = new CobwebZone[8];
        private int _cobwebsCleared;
        private Transform _cam;
        private float _ceilingBonus;

        private class CobwebZone
        {
            public Vector3 pos;
            public GameObject vfx;
            public float dwellTime;
            public bool cleared;
        }

        protected override void OnModeBegun()
        {
            base.OnModeBegun();
            _cam = Camera.main?.transform;
            _cobwebsCleared = 0;
            SpawnCobwebs();
        }

        protected override void OnModeEnded(ChoreResult result)
        {
            foreach (var c in _cobwebs)
                if (c?.vfx != null) Destroy(c.vfx);
            base.OnModeEnded(result);
        }

        protected override void OnTrackedPositionSampled(Vector3 worldPos)
        {
            base.OnTrackedPositionSampled(worldPos);
            if (_cam == null) return;

            // Ceiling bonus: looking up more than 45 degrees
            if (_cam.forward.y > 0.7f)
                _ceilingBonus += Time.deltaTime * 2f;

            // Cobweb dwell check
            foreach (var cw in _cobwebs)
            {
                if (cw == null || cw.cleared) continue;
                if (Vector3.Distance(worldPos, cw.pos) < 0.5f)
                {
                    cw.dwellTime += Time.deltaTime;
                    if (cw.dwellTime >= cobwebDwellSeconds)
                    {
                        cw.cleared = true;
                        _cobwebsCleared++;
                        if (cw.vfx != null) Destroy(cw.vfx);
                    }
                }
            }
        }

        protected override float GetBonusPoints()
        {
            float cobwebRatio = _cobwebs.Length > 0
                ? (float)_cobwebsCleared / _cobwebs.Length
                : 0f;

            // Darkness penalty unless most cobwebs cleared
            float darknessMult = cobwebRatio >= 0.8f ? 1f : 0.8f;

            // Ceiling bonus: up to 200 pts
            float bonus = Mathf.Clamp(_ceilingBonus * 10f, 0f, 200f);
            bonus += cobwebRatio * 150f; // cobweb clear bonus

            return bonus * darknessMult;
        }

        private void SpawnCobwebs()
        {
            if (cobwebPrefab == null) return;
            for (int i = 0; i < cobwebCount && i < _cobwebs.Length; i++)
            {
                float angle = (360f / cobwebCount) * i * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Cos(angle) * 2f, 2f, Mathf.Sin(angle) * 2f);
                _cobwebs[i] = new CobwebZone
                {
                    pos = pos,
                    vfx = Instantiate(cobwebPrefab, pos, Quaternion.identity)
                };
            }
        }
    }
}
