using UnityEngine;
using Chorewars.Integration;

namespace Chorewars.AR
{
    /// <summary>
    /// Applies a resolved home origin pose to a transform, so scans and content can share a stable frame.
    /// </summary>
    public class HomeOriginAligner : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour providerComponent;
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private float retryApplySeconds = 0.5f;

        private IHomeOriginProvider Provider => providerComponent as IHomeOriginProvider;
        private float _nextRetryAt;

        private void Start()
        {
            if (!applyOnStart) return;
            // Synchronous attempt (fast path). Async localization continues in Update().
            ApplyIfAvailable();
        }

        private void Update()
        {
            // Providers may resolve asynchronously (e.g. loading persisted anchors). Retry briefly.
            if (!applyOnStart) return;
            if (Provider == null || !Provider.IsSupported) return;
            if (Time.unscaledTime < _nextRetryAt) return;

            // If not aligned yet, keep trying until resolved.
            if (Provider.TryResolveHomeOrigin(out var pose))
            {
                transform.SetPositionAndRotation(pose.position, pose.rotation);
                applyOnStart = false; // stop retrying once aligned
                return;
            }

            _nextRetryAt = Time.unscaledTime + Mathf.Max(0.05f, retryApplySeconds);
        }

        public bool ApplyIfAvailable()
        {
            var p = Provider;
            if (p == null || !p.IsSupported) return false;
            if (!p.TryResolveHomeOrigin(out var pose)) return false;

            transform.SetPositionAndRotation(pose.position, pose.rotation);
            return true;
        }

        public void SetHomeOriginToCurrent()
        {
            var p = Provider;
            if (p == null || !p.IsSupported) return;

            p.CreateOrUpdateHomeOrigin(new Pose(transform.position, transform.rotation));
            // After saving, try to apply immediately (often still async localize).
            applyOnStart = true;
            _nextRetryAt = 0f;
        }
    }
}

