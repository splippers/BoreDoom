using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private const string PlayerPrefsUuidKey = "boredoom.home_origin_anchor_uuid";

        private OVRSpatialAnchor _runtimeAnchor;
        private Guid _uuid;
        private bool _hasUuid;

        private readonly List<OVRSpatialAnchor.UnboundAnchor> _unboundBuffer = new();

        public bool HasStoredUuid => TryGetStoredUuid(out _);
        public bool IsLocalized => _runtimeAnchor != null && _runtimeAnchor.Localized;
        public Guid Uuid => _uuid;
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
            // Fast path: if we already have a localized runtime anchor, use it.
            if (_runtimeAnchor != null && _runtimeAnchor.Localized)
            {
                homeOriginPose = new Pose(_runtimeAnchor.transform.position, _runtimeAnchor.transform.rotation);
                return true;
            }

            if (!TryGetStoredUuid(out var uuid)) return false;

            // Kick off async load; synchronous API contract can't await here.
            _ = LoadHomeOriginAsync(uuid);
            return false;
#else
            return false;
#endif
        }

        public void CreateOrUpdateHomeOrigin(Pose pose)
        {
#if CHOREWARS_META_XR
            _ = CreateOrUpdateHomeOriginAsync(pose);
#else
            _ = pose;
#endif
        }

#if CHOREWARS_META_XR
        private async void CreateOrUpdateHomeOriginAsync(Pose pose)
        {
            try
            {
                // Replace any previous runtime anchor object.
                if (_runtimeAnchor != null)
                {
                    Destroy(_runtimeAnchor.gameObject);
                    _runtimeAnchor = null;
                }

                var go = new GameObject("HomeOriginAnchor");
                go.transform.SetPositionAndRotation(pose.position, pose.rotation);

                var anchor = go.AddComponent<OVRSpatialAnchor>();

                if (!await anchor.WhenLocalizedAsync())
                {
                    Debug.LogError("[BoreDOOM] Home origin anchor failed to localize.");
                    Destroy(go);
                    return;
                }

                var saveResult = await anchor.SaveAnchorAsync();
                if (!saveResult.Success)
                {
                    Debug.LogError($"[BoreDOOM] Home origin anchor failed to save: {saveResult.Status}");
                    Destroy(go);
                    return;
                }

                _runtimeAnchor = anchor;
                _uuid = anchor.Uuid;
                _hasUuid = true;
                PlayerPrefs.SetString(PlayerPrefsUuidKey, _uuid.ToString("D"));
                PlayerPrefs.Save();

                Debug.Log($"[BoreDOOM] Home origin anchor saved. uuid={_uuid:D}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private async Task LoadHomeOriginAsync(Guid uuid)
        {
            try
            {
                var uuids = new[] { uuid };
                _unboundBuffer.Clear();

                var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, _unboundBuffer);
                if (!result.Success || _unboundBuffer.Count == 0)
                {
                    Debug.LogWarning($"[BoreDOOM] Failed to load home origin anchor: {result.Status}");
                    return;
                }

                var unbound = _unboundBuffer[0];
                if (!await unbound.LocalizeAsync())
                {
                    Debug.LogWarning("[BoreDOOM] Home origin anchor localize failed.");
                    return;
                }

                var go = new GameObject("HomeOriginAnchor_Loaded");
                var anchor = go.AddComponent<OVRSpatialAnchor>();
                unbound.BindTo(anchor);

                if (!await anchor.WhenLocalizedAsync())
                {
                    Debug.LogWarning("[BoreDOOM] Bound home origin anchor not localized yet.");
                    return;
                }

                if (_runtimeAnchor != null && _runtimeAnchor != anchor)
                    Destroy(_runtimeAnchor.gameObject);

                _runtimeAnchor = anchor;
                Debug.Log("[BoreDOOM] Home origin anchor loaded + localized.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private bool TryGetStoredUuid(out Guid uuid)
        {
            uuid = default;
            if (_hasUuid && _uuid != Guid.Empty)
            {
                uuid = _uuid;
                return true;
            }

            if (!PlayerPrefs.HasKey(PlayerPrefsUuidKey)) return false;
            var s = PlayerPrefs.GetString(PlayerPrefsUuidKey, string.Empty);
            if (!Guid.TryParse(s, out var parsed) || parsed == Guid.Empty) return false;

            _uuid = parsed;
            _hasUuid = true;
            uuid = parsed;
            return true;
        }

        private void Awake()
        {
#if CHOREWARS_META_XR
            _ = TryGetStoredUuid(out _);
#endif
        }
#endif
    }
}

