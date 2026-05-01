using UnityEngine;

namespace Chorewars.AR
{
    public class ARSessionManager : MonoBehaviour
    {
        [SerializeField] private bool enablePassthroughByDefault = true;

        private void Start()
        {
            // Placeholder hook point for Meta XR / OpenXR passthrough + spatial mapping init.
            _ = enablePassthroughByDefault;
        }
    }
}
