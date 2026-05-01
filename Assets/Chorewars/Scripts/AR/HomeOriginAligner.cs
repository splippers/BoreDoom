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

        private IHomeOriginProvider Provider => providerComponent as IHomeOriginProvider;

        private void Start()
        {
            if (!applyOnStart) return;
            ApplyIfAvailable();
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
        }
    }
}

