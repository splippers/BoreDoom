using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

namespace Chorewars.Bootstrap
{
    /// <summary>
    /// Prototype scenes were saved as empty YAML (no Camera / XR rig), which yields a black HMD on Quest
    /// even though Unity starts. This builds a minimal tracked rig once and keeps it across scene loads.
    /// Failures here run before the first scene; any exception can prevent rendering — use fallbacks + logs.
    /// </summary>
    public static class QuestXrRigBootstrap
    {
        public const string LogPrefix = "BoreDoom/Startup";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureXrRigBeforeFirstScene()
        {
            Debug.Log($"{LogPrefix}: BeforeSceneLoad (first scene not loaded yet).");

            try
            {
                if (Object.FindObjectsByType<XROrigin>(FindObjectsInactive.Include).Length > 0)
                {
                    Debug.Log($"{LogPrefix}: XROrigin already present, skipping auto rig.");
                    return;
                }

                BuildXrRig();
                Debug.Log($"{LogPrefix}: XR rig created.");
            }
            catch (Exception e)
            {
                Debug.LogError($"{LogPrefix}: XR rig failed, using minimal camera only.\n{e}");
                BuildMinimalCamera();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCameraAfterFirstScene()
        {
            var scene = SceneManager.GetActiveScene();
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
            Debug.Log($"{LogPrefix}: AfterSceneLoad scene=\"{scene.name}\" cameras={cameras.Length}.");

            if (cameras.Length == 0)
            {
                Debug.LogWarning($"{LogPrefix}: No camera after first scene load — creating fallback.");
                BuildMinimalCamera();
            }

            AttachRuntimeDiagnostics();
        }

        private static void AttachRuntimeDiagnostics()
        {
            var diagGo = new GameObject("BoreDoom Xr Diagnostics");
            diagGo.AddComponent<XrHeadsetDiagnostics>();
            Object.DontDestroyOnLoad(diagGo);
        }

        private static void BuildXrRig()
        {
            var xrOriginGo = new GameObject("XR Origin");
            var xrOrigin = xrOriginGo.AddComponent<XROrigin>();

            var floorOffset = new GameObject("Camera Floor Offset");
            floorOffset.transform.SetParent(xrOriginGo.transform, false);

            var camGo = new GameObject("Main Camera");
            try
            {
                camGo.tag = "MainCamera";
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LogPrefix}: Could not set MainCamera tag ({e.Message}).");
            }

            camGo.transform.SetParent(floorOffset.transform, false);

            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.stereoTargetEye = StereoTargetEyeMask.Both;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.1f, 1f);
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 500f;

#if ENABLE_INPUT_SYSTEM
            var poseDriver = camGo.AddComponent<TrackedPoseDriver>();
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            var posAction = new InputAction(type: InputActionType.PassThrough, expectedControlType: "Vector3");
            posAction.AddBinding("<XRHMD>/centerEyePosition");
            var rotAction = new InputAction(type: InputActionType.PassThrough, expectedControlType: "Quaternion");
            rotAction.AddBinding("<XRHMD>/centerEyeRotation");
            posAction.Enable();
            rotAction.Enable();
            poseDriver.positionInput = new InputActionProperty(posAction);
            poseDriver.rotationInput = new InputActionProperty(rotAction);
#endif

            xrOrigin.CameraFloorOffsetObject = floorOffset;
            xrOrigin.Camera = cam;

            Object.DontDestroyOnLoad(xrOriginGo);
        }

        private static void BuildMinimalCamera()
        {
            if (Object.FindObjectsByType<Camera>(FindObjectsInactive.Include).Length > 0)
                return;

            var camGo = new GameObject("Fallback Main Camera");
            try
            {
                camGo.tag = "MainCamera";
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LogPrefix}: Fallback could not set MainCamera tag ({e.Message}).");
            }

            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.stereoTargetEye = StereoTargetEyeMask.Both;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.05f, 0.05f, 1f);
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 500f;

            Object.DontDestroyOnLoad(camGo);
            Debug.Log($"{LogPrefix}: Fallback camera created (magenta-tinted clear for diagnosis).");
        }
    }

    /// <summary>Logs XR display subsystem once — survives log filters better than static-only init when diagnosing black HMD.</summary>
    internal sealed class XrHeadsetDiagnostics : MonoBehaviour
    {
        private bool _logged;

        private void Update()
        {
            if (_logged)
                return;

            _logged = true;
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            var running = displays.Count > 0 && displays[0].running;
            Debug.Log($"{QuestXrRigBootstrap.LogPrefix}: XRDisplaySubsystem count={displays.Count}, running={running}");
        }
    }
}
