using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Chorewars.Core;

namespace Chorewars.UI
{
    /// <summary>
    /// Main mode selection screen. Each mode button calls SelectMode(modeId).
    /// Modes are configured via ModeTile entries in the Inspector.
    /// </summary>
    public class ChoreSelectionUI : MonoBehaviour
    {
        [System.Serializable]
        public class ModeTile
        {
            public string modeId;
            public string displayName;
            public string tagline;
            public string sceneName;
            public Color accentColour = Color.white;
            public Sprite icon;
        }

        [SerializeField] private List<ModeTile> modes = new()
        {
            new() { modeId = "hoover",       displayName = "Standard Clean",    tagline = "Cover every inch",            sceneName = "HooverMode",       accentColour = new Color(0.2f, 0.8f, 1f) },
            new() { modeId = "mowing",       displayName = "Mowing Mode",       tagline = "Tame the lawn",               sceneName = "MowingMode",        accentColour = new Color(0.3f, 0.9f, 0.3f) },
            new() { modeId = "perfect-grid", displayName = "Perfect Grid",      tagline = "Football pitch perfection",   sceneName = "HooverMode",        accentColour = new Color(1f, 0.85f, 0.1f) },
            new() { modeId = "ghost-run",    displayName = "Ghost Run",         tagline = "Beat your best",              sceneName = "HooverMode",        accentColour = new Color(0.7f, 0.5f, 1f) },
            new() { modeId = "chaos-scan",   displayName = "Chaos Assessment",  tagline = "How bad is it really?",       sceneName = "ChaosMode",         accentColour = new Color(1f, 0.3f, 0.2f) },
            new() { modeId = "mow-art",      displayName = "Mowing Art",        tagline = "Clean like a canvas",         sceneName = "HooverMode",        accentColour = new Color(1f, 0.6f, 0.9f) },
            new() { modeId = "declutter",    displayName = "Declutter Dash",    tagline = "Race the clock",              sceneName = "DeclutterMode",     accentColour = new Color(1f, 0.5f, 0.1f) },
            new() { modeId = "battle",       displayName = "ChoreWars Battle",  tagline = "LAN co-op cleaning race",     sceneName = "HooverMode",        accentColour = new Color(0.1f, 1f, 0.7f) },
            new() { modeId = "attic-attack", displayName = "Attic Attack",      tagline = "Fear the dark. Clean it.",    sceneName = "AtticMode",         accentColour = new Color(0.5f, 0.4f, 0.3f) },
            new() { modeId = "shed-dread",   displayName = "Shed Dread",        tagline = "Spiders. Tools. Respect.",    sceneName = "ShedMode",          accentColour = new Color(0.4f, 0.7f, 0.2f) },
            new() { modeId = "basement-bust",displayName = "Basement Bust",     tagline = "Conquer the underworld",      sceneName = "BasementMode",      accentColour = new Color(0.3f, 0.3f, 0.8f) },
            new() { modeId = "garage-barrage",displayName="Garage Barrage",     tagline = "Grease, grime, glory",        sceneName = "GarageMode",        accentColour = new Color(0.6f, 0.6f, 0.6f) },
        };

        [Header("UI")]
        [SerializeField] private Transform tileContainer;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private TMP_Text modeDetailName;
        [SerializeField] private TMP_Text modeDetailTagline;
        [SerializeField] private Button launchButton;

        private ModeTile _selected;

        private void Start()
        {
            BuildTiles();
            if (launchButton != null) launchButton.onClick.AddListener(LaunchSelected);
        }

        private void BuildTiles()
        {
            if (tileContainer == null || tilePrefab == null) return;

            foreach (var m in modes)
            {
                var go = Instantiate(tilePrefab, tileContainer);
                var tile = go.GetComponent<ModeButtonTile>();
                tile?.Init(m, () => OnTileSelected(m));

                // Fallback: wire Button directly
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    var captured = m;
                    btn.onClick.AddListener(() => OnTileSelected(captured));
                }
            }
        }

        private void OnTileSelected(ModeTile m)
        {
            _selected = m;
            if (modeDetailName   != null) modeDetailName.text   = m.displayName;
            if (modeDetailTagline != null) modeDetailTagline.text = m.tagline;
            if (launchButton != null)
            {
                var img = launchButton.GetComponent<Image>();
                if (img != null) img.color = m.accentColour;
            }
        }

        public void SelectMode(string modeId)
        {
            var m = modes.Find(x => x.modeId == modeId);
            if (m != null) OnTileSelected(m);
        }

        private void LaunchSelected()
        {
            if (_selected == null && modes.Count > 0) _selected = modes[0];
            if (_selected == null) return;

            BootstrapSceneRouter.SetLastModeSceneName(_selected.sceneName);
            // Scene is loaded by BootstrapSceneRouter on next Bootstrap load
            UnityEngine.SceneManagement.SceneManager.LoadScene("Bootstrap");
        }
    }

    /// <summary>
    /// Optional component on each tile prefab to receive mode data for rich visuals.
    /// </summary>
    public class ModeButtonTile : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text taglineLabel;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;

        public void Init(ChoreSelectionUI.ModeTile m, System.Action onClick)
        {
            if (nameLabel    != null) nameLabel.text    = m.displayName;
            if (taglineLabel != null) taglineLabel.text = m.tagline;
            if (iconImage    != null && m.icon != null) iconImage.sprite = m.icon;
            if (backgroundImage != null)
            {
                var c = m.accentColour;
                c.a = 0.25f;
                backgroundImage.color = c;
            }
            GetComponent<Button>()?.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
