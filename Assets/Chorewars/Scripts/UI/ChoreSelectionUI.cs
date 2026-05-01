using UnityEngine;
using Chorewars.Modes;
using Chorewars.Core;

namespace Chorewars.UI
{
    public class ChoreSelectionUI : MonoBehaviour
    {
        [SerializeField] private ChoreModeRegistry registry;

        public void SelectMode(string modeId)
        {
            if (registry == null) return;
            if (!registry.TryGet(modeId, out var descriptor)) return;
            _ = descriptor;

            // Scene routing is intentionally lightweight: this avoids coupling selection
            // to ScriptableObject assets while we keep the prototype fork-friendly.
            var sceneName = ModeIdToSceneName(modeId);
            BootstrapSceneRouter.SetLastModeSceneName(sceneName);
        }

        private static string ModeIdToSceneName(string modeId)
        {
            if (string.IsNullOrWhiteSpace(modeId)) return "HooverMode";

            var id = modeId.Trim().ToLowerInvariant();
            if (id.Contains("mow")) return "MowingMode";
            if (id.Contains("hoover") || id.Contains("vac")) return "HooverMode";

            // Other modes currently share the same base scene; we can split scenes later.
            return "HooverMode";
        }
    }
}
