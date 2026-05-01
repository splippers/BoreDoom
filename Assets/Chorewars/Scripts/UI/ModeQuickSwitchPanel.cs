using UnityEngine;
using UnityEngine.SceneManagement;
using Chorewars.Core;

namespace Chorewars.UI
{
    public sealed class ModeQuickSwitchPanel : MonoBehaviour
    {
        private static ModeQuickSwitchPanel _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance != null) return;

            var go = new GameObject(nameof(ModeQuickSwitchPanel));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ModeQuickSwitchPanel>();
        }

        private bool _open = true;

        private void OnGUI()
        {
            if (!_open) return;

            const int w = 320;
            const int h = 170;
            var rect = new Rect(10, 10, w, h);
            GUILayout.BeginArea(rect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mode quick switch");
            if (GUILayout.Button("X", GUILayout.Width(28))) _open = false;
            GUILayout.EndHorizontal();

            var active = SceneManager.GetActiveScene().name;
            GUILayout.Label($"Scene: {active}");
            GUILayout.Label($"Startup default: {BootstrapSceneRouter.GetLastModeSceneNameOrDefault()}");

            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hoover")) SwitchTo("HooverMode");
            if (GUILayout.Button("Mowing")) SwitchTo("MowingMode");
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            if (GUILayout.Button("Reload scene"))
                SwitchTo(active);

            GUILayout.EndArea();
        }

        private static void SwitchTo(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;

            BootstrapSceneRouter.SetLastModeSceneName(sceneName);
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
        }
    }
}

