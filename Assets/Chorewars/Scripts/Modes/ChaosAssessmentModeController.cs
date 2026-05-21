using System.Collections.Generic;
using UnityEngine;
using Chorewars.Dollhouse;

namespace Chorewars.Modes
{
    /// <summary>
    /// Chaos Meter mode: scan your home WITHOUT cleaning anything.
    /// Estimates mess/clutter per room using spatial mesh density.
    /// Drives dollhouse fire/smoke effects proportional to chaos score.
    ///
    /// Not coverage-based — standalone MonoBehaviour.
    /// Mesh density proxy: triangle count per square metre of floor area.
    /// </summary>
    public class ChaosAssessmentModeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DollhouseManager dollhouseManager;
        [SerializeField] private GameObject fireEffectPrefab;   // particle system for chaos rooms
        [SerializeField] private GameObject smokeEffectPrefab;

        [Header("Thresholds (triangles per m²)")]
        [SerializeField] private float messyThreshold = 400f;
        [SerializeField] private float chaosThreshold = 800f;

        // roomId → computed chaos score 0–10
        private readonly Dictionary<string, float> _chaosScores = new();
        private readonly Dictionary<string, GameObject> _effects = new();

        private bool _scanning;

        public float HouseChaosScore { get; private set; }

        public void BeginAssessment()
        {
            _scanning = true;
            _chaosScores.Clear();
            ClearEffects();
        }

        public void EndAssessment()
        {
            _scanning = false;
            ComputeHouseScore();
            ApplyEffects();
        }

        /// <summary>
        /// Called by the spatial mesh subsystem when a room mesh is updated.
        /// roomId: from OVRSceneRoom label. mesh: the current mesh for that room.
        /// floorAreaM2: pre-computed floor area.
        /// </summary>
        public void OnRoomMeshUpdated(string roomId, Mesh mesh, float floorAreaM2)
        {
            if (!_scanning || floorAreaM2 <= 0f) return;

            float density = mesh.triangles.Length / 3f / floorAreaM2;
            float score = Mathf.Lerp(0f, 10f, density / chaosThreshold);
            _chaosScores[roomId] = Mathf.Clamp(score, 0f, 10f);
        }

        private void ComputeHouseScore()
        {
            if (_chaosScores.Count == 0) { HouseChaosScore = 0f; return; }
            float sum = 0f;
            foreach (var v in _chaosScores.Values) sum += v;
            HouseChaosScore = sum / _chaosScores.Count;
        }

        private void ApplyEffects()
        {
            foreach (var kv in _chaosScores)
            {
                string roomId = kv.Key;
                float score = kv.Value;

                // Notify dollhouse — show chaos state
                dollhouseManager?.OnCoverageUpdate(roomId, 0f); // 0% clean = chaotic

                if (score >= 7f && fireEffectPrefab != null)
                {
                    var fx = Instantiate(fireEffectPrefab);
                    _effects[roomId] = fx;
                }
                else if (score >= 4f && smokeEffectPrefab != null)
                {
                    var fx = Instantiate(smokeEffectPrefab);
                    _effects[roomId] = fx;
                }
            }
        }

        private void ClearEffects()
        {
            foreach (var fx in _effects.Values)
                if (fx != null) Destroy(fx);
            _effects.Clear();
        }

        public string GetChaosLabel()
        {
            return HouseChaosScore switch
            {
                >= 9f  => "DOOMED 🔥",
                >= 7f  => "Chaos adjacent 😬",
                >= 5f  => "Lived in (honest)",
                >= 3f  => "Mostly fine",
                _      => "Minimalist — suspicious 🕵️"
            };
        }
    }
}
