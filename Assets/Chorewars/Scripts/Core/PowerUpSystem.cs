using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Chorewars.Core
{
    /// <summary>
    /// Spawns AR power-up orbs during a chore session.
    /// Player walks through them to activate bonuses.
    /// Attach to the same GameObject as the active mode controller.
    /// </summary>
    public class PowerUpSystem : MonoBehaviour
    {
        public enum PowerUpType
        {
            ScoreMultiplier,    // 2x points for 15 seconds
            CoverageBoost,      // +5% instant coverage
            TimeFreeze,         // stops countdown timer for 10 seconds
            GhostTrail,         // reveals ghost path overlay for 20 seconds
            SpeedClean,         // doubles coverage cell size for 30 seconds
        }

        [System.Serializable]
        public class PowerUp
        {
            public PowerUpType type;
            public GameObject prefab;
            public Color colour;
            [HideInInspector] public bool active;
            [HideInInspector] public float expiry;
        }

        [Header("Spawn settings")]
        [SerializeField] private List<PowerUp> powerUps;
        [SerializeField] private float spawnIntervalSeconds = 45f;
        [SerializeField] private float pickupRadiusMeters = 0.5f;
        [SerializeField] private int maxActiveOrbs = 2;

        [Header("HUD")]
        [SerializeField] private TMP_Text activeBuffLabel;

        [Header("Audio")]
        [SerializeField] private AudioClip collectClip;
        [SerializeField] private AudioSource audioSource;

        private readonly List<(PowerUp def, GameObject orb, Vector3 pos)> _orbs = new();
        private Transform _player;
        private float _multiplier = 1f;
        private bool _timeFrozen;

        public float ScoreMultiplier => _multiplier;
        public bool TimeFrozen => _timeFrozen;

        private void Start()
        {
            _player = Camera.main?.transform;
            StartCoroutine(SpawnLoop());
        }

        private void Update()
        {
            if (_player == null) return;

            for (int i = _orbs.Count - 1; i >= 0; i--)
            {
                var (def, orb, pos) = _orbs[i];
                if (orb == null) { _orbs.RemoveAt(i); continue; }

                // Bob animation
                orb.transform.position = pos + Vector3.up * (0.05f * Mathf.Sin(Time.time * 2f + i));
                orb.transform.Rotate(0f, 90f * Time.deltaTime, 0f);

                // Pickup check
                float dist = Vector3.Distance(
                    new Vector3(_player.position.x, pos.y, _player.position.z), pos);
                if (dist < pickupRadiusMeters)
                {
                    Collect(def, orb);
                    _orbs.RemoveAt(i);
                }
            }

            // Expire active buffs
            UpdateBuffExpiry();
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(15f); // grace period at start
            while (true)
            {
                if (_orbs.Count < maxActiveOrbs && powerUps != null && powerUps.Count > 0)
                    SpawnRandomOrb();
                yield return new WaitForSeconds(spawnIntervalSeconds);
            }
        }

        private void SpawnRandomOrb()
        {
            var def = powerUps[Random.Range(0, powerUps.Count)];
            if (def.prefab == null) return;

            // Place in a random position around the player
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist  = Random.Range(1f, 2.5f);
            var pos = new Vector3(
                (_player?.position.x ?? 0f) + Mathf.Cos(angle) * dist,
                0.6f,
                (_player?.position.z ?? 0f) + Mathf.Sin(angle) * dist
            );

            var orb = Instantiate(def.prefab, pos, Quaternion.identity);
            orb.GetComponent<Renderer>()?.material?.SetColor("_Color", def.colour);
            _orbs.Add((def, orb, pos));
        }

        private void Collect(PowerUp def, GameObject orb)
        {
            Destroy(orb);
            if (audioSource != null && collectClip != null)
                audioSource.PlayOneShot(collectClip);

            ApplyBuff(def);
        }

        private void ApplyBuff(PowerUp def)
        {
            switch (def.type)
            {
                case PowerUpType.ScoreMultiplier:
                    _multiplier = 2f;
                    def.active = true;
                    def.expiry = Time.time + 15f;
                    ShowBuff("2x POINTS! 15s");
                    break;
                case PowerUpType.TimeFreeze:
                    _timeFrozen = true;
                    def.active = true;
                    def.expiry = Time.time + 10f;
                    ShowBuff("TIME FROZEN! 10s");
                    break;
                case PowerUpType.SpeedClean:
                    def.active = true;
                    def.expiry = Time.time + 30f;
                    ShowBuff("SPEED CLEAN! 30s");
                    break;
                case PowerUpType.CoverageBoost:
                    ShowBuff("+5% COVERAGE!");
                    // Handled externally by the mode controller checking ScoreMultiplier
                    SendMessage("OnCoverageBoostCollected", 5f, SendMessageOptions.DontRequireReceiver);
                    break;
                case PowerUpType.GhostTrail:
                    def.active = true;
                    def.expiry = Time.time + 20f;
                    ShowBuff("GHOST TRAIL! 20s");
                    SendMessage("OnGhostTrailActivated", SendMessageOptions.DontRequireReceiver);
                    break;
            }
        }

        private void UpdateBuffExpiry()
        {
            if (powerUps == null) return;
            foreach (var pu in powerUps)
            {
                if (!pu.active) continue;
                if (Time.time >= pu.expiry)
                {
                    pu.active = false;
                    switch (pu.type)
                    {
                        case PowerUpType.ScoreMultiplier: _multiplier = 1f; break;
                        case PowerUpType.TimeFreeze: _timeFrozen = false; break;
                    }
                    if (activeBuffLabel != null) activeBuffLabel.text = "";
                }
                else if (activeBuffLabel != null)
                {
                    float remaining = pu.expiry - Time.time;
                    activeBuffLabel.text = $"{pu.type}: {remaining:F0}s";
                }
            }
        }

        private void ShowBuff(string text)
        {
            if (activeBuffLabel != null) activeBuffLabel.text = text;
        }
    }
}
