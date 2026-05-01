using System;
using UnityEngine;

namespace Chorewars.Integration
{
    /// <summary>
    /// Abstraction for persistent anchors needed to build a contiguous multi-session house map.
    ///
    /// Quest 3 support typically comes from Meta XR SDK anchor APIs; this file is intentionally a stub
    /// so the core project can compile without Meta packages.
    /// </summary>
    public class PersistentAnchors : MonoBehaviour
    {
        [Serializable]
        public class AnchorHandle
        {
            public string anchorId;
        }

        public bool IsSupported => false;

        public void CreateOrUpdateHomeOriginAnchor()
        {
            // TODO: Implement via Meta XR persistent anchors.
        }

        public bool TryLoadHomeOriginAnchor(out AnchorHandle handle)
        {
            handle = null;
            return false;
        }
    }
}

