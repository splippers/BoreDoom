using UnityEngine;

namespace Chorewars.Integration
{
    /// <summary>
    /// Meta XR persistent-anchor backed home origin provider.
    ///
    /// This is behind a compile flag so the repo compiles without the Meta XR SDK present.
    /// Define `CHOREWARS_META_XR` in Unity Player Settings when Meta XR packages are installed.
    /// </summary>
    public class MetaXrHomeOriginProvider : MonoBehaviour, IHomeOriginProvider
    {
#if CHOREWARS_META_XR
        // TODO: Implement using Meta XR SDK persistent anchors (OVR / Meta XR Core).
        // Suggested approach:
        // - Create an anchor at the requested Pose (CreateOrUpdateHomeOrigin)
        // - Persist it with a stable UUID
        // - On startup, try to load/locate it (TryResolveHomeOrigin)
        // - Return its world Pose
#endif

        public bool IsSupported
        {
            get
            {
#if CHOREWARS_META_XR
                return true;
#else
                return false;
#endif
            }
        }

        public bool TryResolveHomeOrigin(out Pose homeOriginPose)
        {
            homeOriginPose = default;
#if CHOREWARS_META_XR
            // TODO: load/locate persisted anchor and return pose
            return false;
#else
            return false;
#endif
        }

        public void CreateOrUpdateHomeOrigin(Pose pose)
        {
#if CHOREWARS_META_XR
            // TODO: create/persist anchor
            _ = pose;
#else
            _ = pose;
#endif
        }
    }
}

