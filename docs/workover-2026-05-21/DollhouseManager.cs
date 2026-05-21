using System.Collections.Generic;
using UnityEngine;
using Chorewars.AR;
using Chorewars.Core;

namespace Chorewars.Dollhouse
{
    public enum DollhouseRoomState { Chaotic, InProgress, Clean, Pristine }

    /// <summary>
    /// Central controller for the 3D dollhouse miniature.
    /// Receives coverage updates from the active chore session and drives room visual states.
    /// Place this on the dollhouse pivot object (positioned on a detected plane at 1:20 scale).
    /// </summary>
    public class DollhouseManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DollhouseSnapshotCamera snapshotCamera;
        [SerializeField] private DollhouseHoverTrail hoverTrail;

        [Header("Scale")]
        [SerializeField] private float worldToMiniatureScale = 0.05f; // 1:20

        [Header("Room visuals")]
        [SerializeField] private Material chaoticMaterial;
        [SerializeField] private Material inProgressMaterial;
        [SerializeField] private Material cleanMaterial;
        [SerializeField] private Material pristineMaterial;

        // roomId → state
        private readonly Dictionary<string, DollhouseRoomState> _roomStates = new();
        // roomId → mesh renderer(s) in the dollhouse geometry
        private readonly Dictionary<string, Renderer[]> _roomRenderers = new();

        public IReadOnlyDictionary<string, DollhouseRoomState> RoomStates => _roomStates;

        /// <summary>Called by DollhouseRoomSlicer once rooms are identified from OVRScene.</summary>
        public void RegisterRoom(string roomId, Renderer[] renderers)
        {
            _roomStates[roomId] = DollhouseRoomState.Chaotic;
            _roomRenderers[roomId] = renderers;
            ApplyMaterial(roomId, DollhouseRoomState.Chaotic);
        }

        /// <summary>
        /// Called by the active chore mode (or ChoreSession bridge) whenever coverage updates.
        /// coveragePct is 0–100 for the given room.
        /// </summary>
        public void OnCoverageUpdate(string roomId, float coveragePct)
        {
            if (!_roomStates.ContainsKey(roomId)) return;

            var newState = coveragePct switch
            {
                >= 90f => DollhouseRoomState.Pristine,
                >= 60f => DollhouseRoomState.Clean,
                >= 10f => DollhouseRoomState.InProgress,
                _      => DollhouseRoomState.Chaotic
            };

            if (_roomStates[roomId] == newState) return;

            _roomStates[roomId] = newState;
            ApplyMaterial(roomId, newState);
        }

        /// <summary>
        /// Converts a world-space position from PathTracker into the dollhouse's local space.
        /// Used by DollhouseHoverTrail to position the mini tool.
        /// </summary>
        public Vector3 WorldToDollhouse(Vector3 worldPos)
        {
            return transform.InverseTransformPoint(worldPos) * worldToMiniatureScale;
        }

        public Texture2D CaptureShareCard()
        {
            if (snapshotCamera == null)
            {
                Debug.LogWarning("[Dollhouse] No snapshot camera assigned.");
                return null;
            }
            return snapshotCamera.Capture();
        }

        private void ApplyMaterial(string roomId, DollhouseRoomState state)
        {
            if (!_roomRenderers.TryGetValue(roomId, out var renderers)) return;

            Material mat = state switch
            {
                DollhouseRoomState.Pristine    => pristineMaterial,
                DollhouseRoomState.Clean       => cleanMaterial,
                DollhouseRoomState.InProgress  => inProgressMaterial,
                _                              => chaoticMaterial
            };

            if (mat == null) return;
            foreach (var r in renderers)
                r.sharedMaterial = mat;
        }
    }
}
