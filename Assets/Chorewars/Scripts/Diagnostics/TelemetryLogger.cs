using System;
using System.IO;
using UnityEngine;

namespace Chorewars.Diagnostics
{
    /// <summary>
    /// JSON line logging to <see cref="Application.persistentDataPath"/>/MQ3.log (rotation ~10 MB).
    /// CopilotRef appendix — usable on Quest when adb logcat is thin.
    /// </summary>
    public static class TelemetryLogger
    {
        const string FileName = "MQ3.log";
        const string RotatedName = "MQ3.1.log";
        const long MaxBytes = 10L * 1024 * 1024;

        [Serializable]
        struct Line
        {
            public string ts;
            public string sub;
            public string sev;
            public string msg;
        }

        static readonly object Gate = new object();

        public static void Info(string subsystem, string message) => Write(subsystem, "INFO", message, null);

        public static void Warn(string subsystem, string message) => Write(subsystem, "WARN", message, null);

        public static void Error(string subsystem, string message, Exception ex = null) =>
            Write(subsystem, "ERROR", message, ex);

        static void Write(string subsystem, string severity, string message, Exception ex)
        {
            if (ex != null)
                message = $"{message} | {ex.GetType().Name}: {ex.Message}";

            var line = new Line
            {
                ts = DateTime.UtcNow.ToString("o"),
                sub = subsystem,
                sev = severity,
                msg = message
            };

            var json = JsonUtility.ToJson(line);
            lock (Gate)
            {
                try
                {
                    var dir = Application.persistentDataPath;
                    var path = Path.Combine(dir, FileName);
                    MaybeRotate(path);
                    File.AppendAllText(path, json + "\n");
                }
                catch
                {
                    // Never throw from logging
                }
            }
        }

        static void MaybeRotate(string path)
        {
            if (!File.Exists(path))
                return;
            var info = new FileInfo(path);
            if (info.Length < MaxBytes)
                return;

            var rotated = Path.Combine(Application.persistentDataPath, RotatedName);
            if (File.Exists(rotated))
                File.Delete(rotated);
            File.Move(path, rotated);
        }
    }
}
