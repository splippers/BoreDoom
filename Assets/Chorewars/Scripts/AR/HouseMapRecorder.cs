using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Chorewars.AR
{
    /// <summary>
    /// Records multiple mesh snapshots over time into a "house map" folder.
    /// This is a pragmatic stepping stone toward a Matterport-like reconstruction pipeline.
    /// </summary>
    public class HouseMapRecorder : MonoBehaviour
    {
        [SerializeField] private SpatialMeshTracker meshTracker;
        [SerializeField] private bool recordWhileScanning = true;
        [SerializeField] private float snapshotIntervalSeconds = 15f;
        [SerializeField] private string mapFolderName = "house-map";

        private float _nextSnapshotAt;
        private readonly List<string> _snapshots = new();

        private string MapDir => Path.Combine(Application.persistentDataPath, mapFolderName);

        private void Awake()
        {
            if (meshTracker == null) meshTracker = GetComponentInChildren<SpatialMeshTracker>();
        }

        private void Update()
        {
            if (!recordWhileScanning) return;
            if (meshTracker == null) return;
            if (snapshotIntervalSeconds <= 0f) return;
            if (Time.unscaledTime < _nextSnapshotAt) return;

            _nextSnapshotAt = Time.unscaledTime + snapshotIntervalSeconds;
            TakeSnapshot();
        }

        public void Begin()
        {
            Directory.CreateDirectory(MapDir);
            _snapshots.Clear();
            _nextSnapshotAt = 0f;
        }

        public void End()
        {
            WriteManifest();
        }

        public void TakeSnapshot()
        {
            Directory.CreateDirectory(MapDir);
            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var objPath = meshTracker.ExportCurrentSnapshotAsObj(Path.Combine(mapFolderName, $"snapshot-{ts}"));
            _snapshots.Add(objPath);
            WriteManifest();
        }

        private void WriteManifest()
        {
            Directory.CreateDirectory(MapDir);
            var manifestPath = Path.Combine(MapDir, "manifest.txt");

            using var sw = new StreamWriter(manifestPath, false);
            sw.WriteLine("# BoreDOOM house-map manifest");
            sw.WriteLine($"# updated_utc {DateTime.UtcNow:O}");
            foreach (var s in _snapshots) sw.WriteLine(s);
        }
    }
}

