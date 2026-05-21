using System.Collections.Generic;
using UnityEngine;
using Chorewars.AR;

namespace Chorewars.Modes
{
    /// <summary>
    /// Mowing Art mode: vacuum/mow your room in decorative patterns.
    /// AR overlay renders the target pattern; player traces it for bonus points.
    /// Patterns: Stripes, Diamond, Spiral, Chevron.
    /// </summary>
    public class MowingArtModeController : BaseCoverageModeController
    {
        public enum ArtPattern { Stripes, Diamond, Spiral, Chevron }

        [Header("Art pattern settings")]
        [SerializeField] private ArtPattern selectedPattern = ArtPattern.Stripes;
        [SerializeField] private float patternCellSize = 0.4f;
        [SerializeField] private Material patternOverlayMat;
        [SerializeField] private Color activeColour   = new Color(0.9f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color completedColour = new Color(0.2f, 0.9f, 0.4f, 0.7f);

        private readonly List<PatternCell> _cells = new();
        private int _cellsCompleted;

        private class PatternCell
        {
            public Vector2Int coord;
            public bool completed;
            public GameObject visual;
        }

        protected override void OnModeBegun()
        {
            base.OnModeBegun();
            GeneratePatternCells();
        }

        protected override void OnModeEnded(Core.ChoreResult result)
        {
            base.OnModeEnded(result);
            foreach (var c in _cells)
                if (c.visual != null) Destroy(c.visual);
        }

        protected override void OnTrackedPositionSampled(Vector3 worldPos)
        {
            base.OnTrackedPositionSampled(worldPos);
            MarkCellAt(worldPos);
        }

        protected override float GetBonusPoints()
        {
            if (_cells.Count == 0) return 0f;
            float pct = (float)_cellsCompleted / _cells.Count;
            return pct * 500f; // up to 500 art bonus
        }

        private void GeneratePatternCells()
        {
            _cells.Clear();
            _cellsCompleted = 0;

            var bounds = new Bounds(Vector3.zero, new Vector3(5f, 0f, 5f)); // TODO: real room bounds
            int countX = Mathf.CeilToInt(bounds.size.x / patternCellSize);
            int countZ = Mathf.CeilToInt(bounds.size.z / patternCellSize);

            for (int x = 0; x < countX; x++)
            for (int z = 0; z < countZ; z++)
            {
                if (!IsCellInPattern(x, z, countX, countZ)) continue;

                var cell = new PatternCell { coord = new Vector2Int(x, z) };
                if (patternOverlayMat != null)
                {
                    cell.visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    cell.visual.transform.position = new Vector3(
                        bounds.min.x + (x + 0.5f) * patternCellSize,
                        0.01f,
                        bounds.min.z + (z + 0.5f) * patternCellSize
                    );
                    cell.visual.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    cell.visual.transform.localScale = Vector3.one * patternCellSize * 0.95f;
                    cell.visual.GetComponent<Renderer>().material = Instantiate(patternOverlayMat);
                    cell.visual.GetComponent<Renderer>().material.color = activeColour;
                }
                _cells.Add(cell);
            }
        }

        private bool IsCellInPattern(int x, int z, int countX, int countZ)
        {
            return selectedPattern switch
            {
                ArtPattern.Stripes   => z % 2 == 0,
                ArtPattern.Diamond   => (x + z) % 2 == 0,
                ArtPattern.Chevron   => (x % 3 == 0) || (z % 3 == (x / 3) % 3),
                ArtPattern.Spiral    => true, // all cells; spiral order tracked separately
                _                    => true
            };
        }

        private void MarkCellAt(Vector3 worldPos)
        {
            var bounds = new Bounds(Vector3.zero, new Vector3(5f, 0f, 5f));
            int cx = Mathf.FloorToInt((worldPos.x - bounds.min.x) / patternCellSize);
            int cz = Mathf.FloorToInt((worldPos.z - bounds.min.z) / patternCellSize);
            var coord = new Vector2Int(cx, cz);

            foreach (var cell in _cells)
            {
                if (cell.completed || cell.coord != coord) continue;
                cell.completed = true;
                _cellsCompleted++;
                if (cell.visual != null)
                    cell.visual.GetComponent<Renderer>().material.color = completedColour;
                break;
            }
        }
    }
}
