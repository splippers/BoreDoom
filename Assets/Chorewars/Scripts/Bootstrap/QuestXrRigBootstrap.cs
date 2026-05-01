using Unity.XR.CoreUtils;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

namespace Chorewars.Bootstrap
{
    /// <summary>
    /// Prototype scenes were saved as empty YAML (no Camera / XR rig), which yields a black HMD on Quest
    /// even though Unity starts. This builds a minimal tracked rig once and keeps it across scene loads.
    /// </summary>
    public static class QuestXrRigBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureXrRig()
        {
            if (Object.FindObjectsByType<XROrigin>(FindObjectsInactive.Include).Length > 0)
                return;

            var xrOriginGo = new GameObject("XR Origin");
            var xrOrigin = xrOriginGo.AddComponent<XROrigin>();

            var floorOffset = new GameObject("Camera Floor Offset");
            floorOffset.transform.SetParent(xrOriginGo.transform, false);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(floorOffset.transform, false);

            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
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
    }
}
