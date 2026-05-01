using System;
using UnityEngine;

namespace Chorewars.Integration
{
    /// <summary>
    /// Abstraction for persistent anchors needed to build a contiguous multi-session house map.
    ///
    /// Quest 3 support typically comes from Meta XR SDK anchor APIs; this file is intentionally a stub
    /// so the core project can compile without Meta packages.
    ///
    /// For real devices, prefer <see cref="MetaXrHomeOriginProvider"/> behind `CHOREWARS_META_XR`.
    /// </summary>
    public class PersistentAnchors : MonoBehaviour, IHomeOriginProvider
    {
        [Serializable]
        public class AnchorHandle
        {
            public string anchorId;
        }

        public bool IsSupported => false;

        public void CreateOrUpdateHomeOrigin(Pose pose)
        {
            // TODO: Implement via Meta XR persistent anchors.
            _ = pose;
        }

        public bool TryResolveHomeOrigin(out Pose homeOriginPose)
        {
            homeOriginPose = default;
            return false;
        }
    }
}

