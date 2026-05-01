using UnityEngine;

namespace Chorewars.AR
{
    public class PathLineVisualizer : MonoBehaviour
    {
        [SerializeField] private PathTracker pathTracker;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int maxPoints = 4000;
        [SerializeField] private int updateEveryNFrames = 2;

        private int _lastCount;

        private void Awake()
        {
            if (pathTracker == null) pathTracker = GetComponentInChildren<PathTracker>();
            if (lineRenderer == null) lineRenderer = GetComponentInChildren<LineRenderer>();
        }

        private void Update()
        {
            if (updateEveryNFrames > 1 && (Time.frameCount % updateEveryNFrames) != 0) return;
            if (pathTracker == null || lineRenderer == null) return;

            int count = pathTracker.worldPositions.Count;
            if (count == _lastCount && count > 0) return;
            _lastCount = count;

            int n = Mathf.Min(count, maxPoints);
            lineRenderer.positionCount = n;

            int start = Mathf.Max(0, count - n);
            for (int i = 0; i < n; i++)
            {
                lineRenderer.SetPosition(i, pathTracker.worldPositions[start + i]);
            }
        }
    }
}

