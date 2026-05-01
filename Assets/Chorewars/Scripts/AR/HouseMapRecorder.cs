using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Chorewars.Tools;
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
        [SerializeField] private Transform scanRoot;
        [SerializeField] private bool recordWhileScanning = true;
        [SerializeField] private float snapshotIntervalSeconds = 15f;
        [SerializeField] private string mapFolderName = "house-map";

        private float _nextSnapshotAt;
        private readonly List<string> _snapshots = new();
        private readonly List<ManifestEntry> _entries = new();

        private string MapDir => Path.Combine(Application.persistentDataPath, mapFolderName);

        [Serializable]
        public class ManifestEntry
        {
            public string timestampUtc;
            public string objAbsolutePath;
            public string objRelativePersistentPath;
            public int meshCount;
            public float coveragePercent;
            public Vector3 scanRootPosition;
            public Quaternion scanRootRotation;
            public string homeAnchorUuid;
        }

        private void Awake()
        {
            if (meshTracker == null) meshTracker = GetComponentInChildren<SpatialMeshTracker>();
            if (scanRoot == null) scanRoot = meshTracker != null ? meshTracker.transform : null;
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
            _entries.Clear();
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
            var objPath = meshTracker.ExportCurrentSnapshotAsObj($"snapshot-{ts}");

            _snapshots.Add(objPath);

            var entry = new ManifestEntry
            {
                timestampUtc = DateTime.UtcNow.ToString("O"),
                objAbsolutePath = objPath,
                objRelativePersistentPath = ToRelativePersistentPath(objPath),
                meshCount = meshTracker != null ? meshTracker.MeshCount : 0,
                coveragePercent = 0f,
                scanRootPosition = scanRoot != null ? scanRoot.position : Vector3.zero,
                scanRootRotation = scanRoot != null ? scanRoot.rotation : Quaternion.identity,
                homeAnchorUuid = ReadHomeAnchorUuid()
            };

            _entries.Add(entry);
            WriteManifest();
        }

        public string MergeAllSnapshotsToSingleObj(string filenamePrefix = "merged-house-map")
        {
            Directory.CreateDirectory(MapDir);
            if (_snapshots.Count == 0)
            {
                Debug.LogWarning("[BoreDOOM] No snapshots available to merge.");
                return null;
            }

            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var outPath = Path.Combine(Application.persistentDataPath, $"{filenamePrefix}-{ts}.obj");
            return HouseMapExporter.MergeObjFiles(_snapshots, outPath);
        }

        public string PackageHouseMapZip(string filenamePrefix = "house-map-package")
        {
            Directory.CreateDirectory(MapDir);

            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var zipPath = Path.Combine(Application.persistentDataPath, $"{filenamePrefix}-{ts}.zip");

            if (File.Exists(zipPath)) File.Delete(zipPath);

            // Ensure manifests are up to date on disk.
            WriteManifest();

            ZipFile.CreateFromDirectory(MapDir, zipPath, System.IO.Compression.CompressionLevel.Fastest, includeBaseDirectory: false);
            return zipPath;
        }

        private void WriteManifest()
        {
            Directory.CreateDirectory(MapDir);
            var manifestPath = Path.Combine(MapDir, "manifest.jsonl");

            using var sw = new StreamWriter(manifestPath, false);
            foreach (var e in _entries)
            {
                sw.WriteLine(JsonUtility.ToJson(e));
            }

            // Also keep a simple text list for humans.
            var txtPath = Path.Combine(MapDir, "manifest-paths.txt");
            using var sw2 = new StreamWriter(txtPath, false);
            sw2.WriteLine("# BoreDOOM house-map snapshot paths");
            sw2.WriteLine($"# updated_utc {DateTime.UtcNow:O}");
            foreach (var s in _snapshots) sw2.WriteLine(s);
        }

        private static string ToRelativePersistentPath(string absolutePath)
        {
            try
            {
                var root = Application.persistentDataPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (absolutePath.StartsWith(root, StringComparison.Ordinal))
                    return absolutePath.Substring(root.Length);
            }
            catch
            {
                // ignore
            }

            return absolutePath;
        }

        private static string ReadHomeAnchorUuid()
        {
            try
            {
                const string key = "boredoom.home_origin_anchor_uuid";
                return PlayerPrefs.GetString(key, string.Empty);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}

