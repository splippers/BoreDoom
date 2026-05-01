using System.Collections.Generic;
using UnityEngine;

namespace Chorewars.AR
{
    public class CoverageMap : MonoBehaviour
    {
        [SerializeField] private float cellSizeMeters = 0.25f;
        [SerializeField] private bool useFixedBounds = true;
        [SerializeField] private Vector2 fixedMinXZ = new(-2f, -2f);
        [SerializeField] private Vector2 fixedMaxXZ = new(2f, 2f);

        private readonly HashSet<Vector2Int> _visited = new();
        private Vector2 _min;
        private Vector2 _max;

        private void Awake()
        {
            _min = fixedMinXZ;
            _max = fixedMaxXZ;
        }

        public void MarkVisitedWorldPosition(Vector3 worldPos)
        {
            var xz = new Vector2(worldPos.x, worldPos.z);
            if (!useFixedBounds)
            {
                _min = Vector2.Min(_min, xz);
                _max = Vector2.Max(_max, xz);
            }

            var cell = WorldToCell(xz);
            _visited.Add(cell);
        }

        public float ComputeCoveragePercent()
        {
            if (cellSizeMeters <= 0f) return 0f;

            var minCell = WorldToCell(_min);
            var maxCell = WorldToCell(_max);

            int width = Mathf.Abs(maxCell.x - minCell.x) + 1;
            int height = Mathf.Abs(maxCell.y - minCell.y) + 1;
            int total = Mathf.Max(1, width * height);

            return Mathf.Clamp01(_visited.Count / (float)total) * 100f;
        }

        private Vector2Int WorldToCell(Vector2 xz)
        {
            int cx = Mathf.FloorToInt(xz.x / cellSizeMeters);
            int cz = Mathf.FloorToInt(xz.y / cellSizeMeters);
            return new Vector2Int(cx, cz);
        }
    }
}
