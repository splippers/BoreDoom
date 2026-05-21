using System.Collections.Generic;
using UnityEngine;
using Chorewars.AR;

namespace Chorewars.Dollhouse
{
    /// <summary>
    /// Renders a mini tool trail inside the dollhouse following the real-world PathTracker positions.
    /// Attach to a LineRenderer on the dollhouse. Assign pathTracker and dollhouseManager.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class DollhouseHoverTrail : MonoBehaviour
    {
        [SerializeField] private PathTracker pathTracker;
        [SerializeField] private DollhouseManager dollhouseManager;
        [SerializeField] private float trailHeightLocal = 0.01f; // sits just above the floor

        private LineRenderer _line;
        private int _lastPointCount;

        private void Awake() => _line = GetComponent<LineRenderer>();

        private void LateUpdate()
        {
            if (pathTracker == null || dollhouseManager == null) return;

            var positions = pathTracker.worldPositions;
            if (positions.Count == _lastPointCount) return;

            _lastPointCount = positions.Count;
            _line.positionCount = positions.Count;

            for (int i = 0; i < positions.Count; i++)
            {
                var local = dollhouseManager.WorldToDollhouse(positions[i]);
                local.y = trailHeightLocal;
                _line.SetPosition(i, transform.TransformPoint(local));
            }
        }
    }
}
