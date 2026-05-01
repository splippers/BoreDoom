using Chorewars.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chorewars.Core
{
    public static class BootstrapSceneRouter
    {
        private const string PrefKey = "Chorewars.LastModeScene";
        private const string BootstrapSceneName = "Bootstrap";
        private const string DefaultModeSceneName = "HooverMode";

        public static string GetLastModeSceneNameOrDefault()
        {
            var scene = PlayerPrefs.GetString(PrefKey, DefaultModeSceneName);
            return string.IsNullOrWhiteSpace(scene) ? DefaultModeSceneName : scene;
        }

        public static void SetLastModeSceneName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            PlayerPrefs.SetString(PrefKey, sceneName);
            PlayerPrefs.Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoRouteFromBootstrap()
        {
            var active = SceneManager.GetActiveScene();
            if (!active.IsValid()) return;
            if (!string.Equals(active.name, BootstrapSceneName, System.StringComparison.OrdinalIgnoreCase)) return;

            var target = GetLastModeSceneNameOrDefault();
            if (string.Equals(target, BootstrapSceneName, System.StringComparison.OrdinalIgnoreCase)) target = DefaultModeSceneName;

            Debug.Log($"{QuestXrRigBootstrap.LogPrefix}: routing Bootstrap → {target}.");

            if (Application.CanStreamedLevelBeLoaded(target))
                SceneManager.LoadScene(target);
            else
                SceneManager.LoadScene(DefaultModeSceneName);
        }
    }
}
