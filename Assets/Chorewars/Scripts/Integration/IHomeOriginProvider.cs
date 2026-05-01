using UnityEngine;

namespace Chorewars.Integration
{
    /// <summary>
    /// Provides a stable "home origin" transform for aligning spatial meshes into a contiguous map.
    /// Implementations may use Meta XR persistent anchors (recommended), OpenXR spatial anchors, etc.
    /// </summary>
    public interface IHomeOriginProvider
    {
        bool IsSupported { get; }

        /// <summary>
        /// Attempt to load/resolve a persisted origin. Returns true if ready.
        /// </summary>
        bool TryResolveHomeOrigin(out Pose homeOriginPose);

        /// <summary>
        /// Create/update the persisted origin at the current pose.
        /// </summary>
        void CreateOrUpdateHomeOrigin(Pose pose);
    }
}

