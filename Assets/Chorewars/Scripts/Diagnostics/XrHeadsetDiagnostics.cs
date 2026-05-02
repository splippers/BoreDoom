using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Chorewars.Diagnostics
{
    /// <summary>Logs XR display subsystem once; updates <see cref="SubsystemHealth"/> and <see cref="TelemetryLogger"/>.</summary>
    public sealed class XrHeadsetDiagnostics : MonoBehaviour
    {
        private bool _done;

        private void Update()
        {
            if (_done)
                return;

            _done = true;
            SubsystemHealth.XrDisplay = SubsystemStatus.Initialising;

            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            var running = displays.Count > 0 && displays[0].running;
            var msg = $"XRDisplaySubsystem count={displays.Count}, running={running}";
            Debug.Log($"{Chorewars.Bootstrap.QuestXrRigBootstrap.LogPrefix}: {msg}");
            TelemetryLogger.Info("XR", msg);

            SubsystemHealth.XrDisplay = running ? SubsystemStatus.Ok : SubsystemStatus.Failed;
            if (!running)
                TelemetryLogger.Warn("XR", "XRDisplaySubsystem not running — HMD may stay black; check OpenXR Android features (Meta Quest Support).");
        }
    }
}
