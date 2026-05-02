namespace Chorewars.Diagnostics
{
    public enum SubsystemStatus
    {
        NotStarted,
        Initialising,
        Ok,
        Failed
    }

    /// <summary>
    /// Coarse runtime health for in-headset HUD / future telemetry (CopilotRef).
    /// Updated by bootstrap and diagnostics; other systems can assign as features land.
    /// </summary>
    public static class SubsystemHealth
    {
        public static SubsystemStatus XrDisplay { get; set; } = SubsystemStatus.NotStarted;
        public static SubsystemStatus BootstrapRig { get; set; } = SubsystemStatus.NotStarted;
        public static SubsystemStatus Passthrough { get; set; } = SubsystemStatus.NotStarted;
        public static SubsystemStatus ArCamera { get; set; } = SubsystemStatus.NotStarted;
        public static SubsystemStatus Hud { get; set; } = SubsystemStatus.NotStarted;
        public static SubsystemStatus ModeController { get; set; } = SubsystemStatus.NotStarted;
        public static SubsystemStatus PathTracker { get; set; } = SubsystemStatus.NotStarted;
        public static SubsystemStatus CoverageMap { get; set; } = SubsystemStatus.NotStarted;
    }
}
