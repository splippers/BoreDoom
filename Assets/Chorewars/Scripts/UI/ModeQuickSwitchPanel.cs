using UnityEngine;
using UnityEngine.SceneManagement;
using Chorewars.Core;
using UnityEngine.XR;

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
        private int _selected;
        private float _nextNavAt;

        private void Update()
        {
            if (!_open) return;

            // IMGUI doesn't receive VR "mouse" clicks on device.
            // Provide controller navigation: thumbstick left/right selects, A activates, B closes.
            // Also provide hands-only pinch: left pinch cycles, right pinch activates.
            Vector2 axis = default;
            bool primaryDown = false;
            bool secondaryDown = false;
            bool hasControllerInput = TryGetPrimary2DAxis(out axis) && TryGetPrimaryButtonDown(out primaryDown, out secondaryDown);

            bool leftPinchDown = false;
            bool rightPinchDown = false;
            bool hasPinchInput = HandPinchInput.TryGetPinchDown(out leftPinchDown, out rightPinchDown);

            float now = Time.unscaledTime;
            if (now >= _nextNavAt)
            {
                if (hasControllerInput)
                {
                    if (axis.x <= -0.65f) { _selected = (_selected + 2) % 3; _nextNavAt = now + 0.22f; }
                    else if (axis.x >= 0.65f) { _selected = (_selected + 1) % 3; _nextNavAt = now + 0.22f; }
                }

                if (hasPinchInput && leftPinchDown)
                {
                    _selected = (_selected + 1) % 3;
                    _nextNavAt = now + 0.22f;
                }
            }

            if (hasControllerInput && secondaryDown) _open = false;
            if (hasPinchInput && rightPinchDown == false && (!hasControllerInput || !primaryDown)) return;
            if (hasControllerInput && !primaryDown && (!hasPinchInput || !rightPinchDown)) return;

            var active = SceneManager.GetActiveScene().name;
            switch (_selected)
            {
                case 0: SwitchTo("HooverMode"); break;
                case 1: SwitchTo("MowingMode"); break;
                case 2: SwitchTo(active); break;
            }
        }

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
            if (GUILayout.Button((_selected == 0 ? "> " : "") + "Hoover")) SwitchTo("HooverMode");
            if (GUILayout.Button((_selected == 1 ? "> " : "") + "Mowing")) SwitchTo("MowingMode");
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            if (GUILayout.Button((_selected == 2 ? "> " : "") + "Reload scene"))
                SwitchTo(active);

            GUILayout.Space(6);
            GUILayout.Label("Controller: stick L/R select, A activate, B close");
            GUILayout.Label("Hands: left pinch cycles, right pinch activates");

            GUILayout.EndArea();
        }

        private static void SwitchTo(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;

            BootstrapSceneRouter.SetLastModeSceneName(sceneName);
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
        }

        private static bool TryGetPrimary2DAxis(out Vector2 axis)
        {
            axis = default;
#if CHOREWARS_META_XR
            axis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
            return true;
#else
            var dev = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            return dev.isValid && dev.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis);
#endif
        }

        private static bool TryGetPrimaryButtonDown(out bool primaryDown, out bool secondaryDown)
        {
            primaryDown = false;
            secondaryDown = false;

#if CHOREWARS_META_XR
            primaryDown = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);     // A
            secondaryDown = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);   // B
            return true;
#else
            var dev = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (!dev.isValid) return false;
            dev.TryGetFeatureValue(CommonUsages.primaryButton, out bool a);
            dev.TryGetFeatureValue(CommonUsages.secondaryButton, out bool b);
            primaryDown = a;
            secondaryDown = b;
            return true;
#endif
        }
    }
}

