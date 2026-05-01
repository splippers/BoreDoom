using System.Collections.Generic;
using UnityEngine;

namespace Chorewars.AR
{
    public class PathTracker : MonoBehaviour
    {
        public readonly List<Vector3> worldPositions = new();

        [SerializeField] private float minDistanceMeters = 0.05f;

        private Vector3 _lastPos;

        private void Start()
        {
            _lastPos = transform.position;
            worldPositions.Add(_lastPos);
        }

        private void Update()
        {
            var current = transform.position;
            if (Vector3.Distance(current, _lastPos) < minDistanceMeters) return;

            worldPositions.Add(current);
            _lastPos = current;
        }
    }
}
