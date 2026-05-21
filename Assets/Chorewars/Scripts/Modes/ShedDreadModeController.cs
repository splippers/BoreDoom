using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Chorewars.Core;

namespace Chorewars.Modes
{
    /// <summary>
    /// Shed Dread: organise a chaotic shed by "filing" tools to their zones.
    ///
    /// Unique mechanics:
    /// - AR tool icons float around the shed at random positions
    /// - 6 coloured storage zones on the shed floor/walls
    /// - Player must carry each tool (dwell 1.5s on it) then place it in the correct zone
    /// - Misplaced tools cost 20 points
    /// - Bonus: combo multiplier for consecutive correct placements
    /// - Spider encounter: random spider AR spawns — dwell to "shoo" it away (+10 pts)
    /// </summary>
    public class ShedDreadModeController : BaseCoverageModeController
    {
        [Header("Shed Dread settings")]
        [SerializeField] private int toolCount = 10;
        [SerializeField] private float pickupDwellSeconds = 1.5f;
        [SerializeField] private GameObject toolPrefab;
        [SerializeField] private GameObject zonePrefab;
        [SerializeField] private GameObject spiderPrefab;
        [SerializeField] private TMP_Text comboLabel;
        [SerializeField] private float spiderSpawnChance = 0.15f; // per minute

        private readonly List<ShedTool> _tools = new();
        private readonly List<ShedZone> _zones = new();
        private ShedTool _heldTool;
        private int _combo;
        private int _organisationScore;
        private float _spiderTimer;

        private static readonly string[] ToolNames = { "Hammer", "Spanner", "Drill", "Rake", "Shovel", "Saw", "Tape", "Brush", "Mallet", "Pliers" };
        private static readonly Color[] ZoneColours = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan };

        private class ShedTool
        {
            public string name;
            public int targetZone;
            public Vector3 pos;
            public GameObject vfx;
            public float dwellTime;
            public bool placed;
        }

        private class ShedZone
        {
            public int id;
            public Vector3 pos;
            public GameObject vfx;
            public Color colour;
        }

        protected override void OnModeBegun()
        {
            base.OnModeBegun();
            _combo = 0; _organisationScore = 0;
            SpawnZones();
            SpawnTools();
        }

        protected override void OnModeEnded(ChoreResult result)
        {
            foreach (var t in _tools) if (t.vfx != null) Destroy(t.vfx);
            foreach (var z in _zones) if (z.vfx != null) Destroy(z.vfx);
            base.OnModeEnded(result);
        }

        protected override void OnTrackedPositionSampled(Vector3 worldPos)
        {
            base.OnTrackedPositionSampled(worldPos);

            // Spider encounter timer
            _spiderTimer += Time.deltaTime;
            if (_spiderTimer > 60f)
            {
                _spiderTimer = 0f;
                if (Random.value < spiderSpawnChance) TriggerSpider(worldPos);
            }

            if (_heldTool == null)
            {
                // Try to pick up nearest unplaced tool
                foreach (var t in _tools)
                {
                    if (t.placed) continue;
                    if (Vector3.Distance(worldPos, t.pos) < 0.4f)
                    {
                        t.dwellTime += Time.deltaTime;
                        if (t.dwellTime >= pickupDwellSeconds)
                        {
                            _heldTool = t;
                            if (comboLabel != null) comboLabel.text = $"Holding: {t.name}";
                        }
                    }
                }
            }
            else
            {
                // Try to place in nearest zone
                foreach (var z in _zones)
                {
                    if (Vector3.Distance(worldPos, z.pos) < 0.5f)
                    {
                        PlaceTool(_heldTool, z);
                        break;
                    }
                }
            }
        }

        private void PlaceTool(ShedTool tool, ShedZone zone)
        {
            tool.placed = true;
            _heldTool = null;
            if (tool.vfx != null) Destroy(tool.vfx);

            bool correct = zone.id == tool.targetZone;
            if (correct)
            {
                _combo++;
                _organisationScore += 50 + (_combo * 10);
                if (comboLabel != null) comboLabel.text = $"COMBO x{_combo}! +{50 + _combo * 10}";
            }
            else
            {
                _combo = 0;
                _organisationScore -= 20;
                if (comboLabel != null) comboLabel.text = "Wrong zone! -20";
            }
        }

        private void TriggerSpider(Vector3 nearPos)
        {
            if (spiderPrefab == null) return;
            var spiderPos = nearPos + new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            var spider = Instantiate(spiderPrefab, spiderPos, Quaternion.identity);
            Destroy(spider, 8f); // auto-despawn if not addressed
            _organisationScore += 10;
        }

        protected override float GetBonusPoints() => Mathf.Max(0f, _organisationScore);

        private void SpawnZones()
        {
            _zones.Clear();
            for (int i = 0; i < 6; i++)
            {
                float angle = (360f / 6f) * i * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Cos(angle) * 2f, 0f, Mathf.Sin(angle) * 2f);
                var z = new ShedZone { id = i, pos = pos, colour = ZoneColours[i] };
                if (zonePrefab != null)
                {
                    z.vfx = Instantiate(zonePrefab, pos, Quaternion.identity);
                    z.vfx.GetComponent<Renderer>()?.material?.SetColor("_Color", ZoneColours[i]);
                }
                _zones.Add(z);
            }
        }

        private void SpawnTools()
        {
            _tools.Clear();
            for (int i = 0; i < toolCount; i++)
            {
                var pos = new Vector3(Random.Range(-2f, 2f), 0.3f, Random.Range(-2f, 2f));
                var t = new ShedTool
                {
                    name = ToolNames[i % ToolNames.Length],
                    targetZone = i % _zones.Count,
                    pos = pos
                };
                if (toolPrefab != null)
                    t.vfx = Instantiate(toolPrefab, pos, Quaternion.identity);
                _tools.Add(t);
            }
        }
    }
}
