using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Chorewars.AR
{
    /// <summary>
    /// Minimal environment mesh capture wrapper.
    ///
    /// On Quest 3, the actual availability/quality of meshing depends on runtime + Meta XR configuration.
    /// This script is written to compile on plain Unity/OpenXR and will capture meshes when exposed via
    /// an `XRMeshSubsystem`.
    /// </summary>
    public class SpatialMeshTracker : MonoBehaviour
    {
        [Header("Capture")]
        [SerializeField] private bool startScanningOnEnable;
        [SerializeField] private float pollIntervalSeconds = 1.0f;

        [Header("Mesh")]
        [SerializeField] private MeshCollider meshColliderPrefab;
        [SerializeField] private bool generateColliders = false;

        private readonly List<XRMeshSubsystem> _subsystems = new();
        private readonly Dictionary<MeshId, Mesh> _meshesById = new();
        private readonly Dictionary<MeshId, GameObject> _meshGosById = new();

        private bool _scanning;
        private float _nextPollAt;

        private void OnEnable()
        {
            RefreshSubsystems();
            if (startScanningOnEnable) StartScanning();
        }

        private void OnDisable()
        {
            StopScanning();
        }

        public void RefreshSubsystems()
        {
            _subsystems.Clear();
            SubsystemManager.GetInstances(_subsystems);
        }

        public void StartScanning()
        {
            _scanning = true;
            _nextPollAt = 0f;
        }

        public void StopScanning()
        {
            _scanning = false;
        }

        private void Update()
        {
            if (!_scanning) return;
            if (pollIntervalSeconds <= 0f) return;
            if (Time.unscaledTime < _nextPollAt) return;

            _nextPollAt = Time.unscaledTime + pollIntervalSeconds;
            PollMeshInfosAndRequestMeshes();
        }

        private void PollMeshInfosAndRequestMeshes()
        {
            if (_subsystems.Count == 0) RefreshSubsystems();
            if (_subsystems.Count == 0) return;

            foreach (var s in _subsystems)
            {
                if (s == null) continue;
                if (!s.running) s.Start();

                var infos = new List<MeshInfo>();
                s.GetMeshInfos(infos);

                foreach (var info in infos)
                {
                    if (info.ChangeState == MeshChangeState.Removed)
                    {
                        RemoveMesh(info.MeshId);
                        continue;
                    }

                    RequestMesh(s, info.MeshId);
                }
            }
        }

        private void RemoveMesh(MeshId id)
        {
            if (_meshGosById.TryGetValue(id, out var go) && go != null)
            {
                Destroy(go);
            }

            _meshGosById.Remove(id);
            _meshesById.Remove(id);
        }

        private void RequestMesh(XRMeshSubsystem subsystem, MeshId id)
        {
            if (_meshesById.TryGetValue(id, out var existing) && existing != null)
            {
                // Still request updates; runtime can refine topology.
            }

            var mesh = existing != null ? existing : new Mesh { indexFormat = IndexFormat.UInt32 };
            var attributes = MeshVertexAttributes.Normals;

            subsystem.GenerateMeshAsync(
                id,
                mesh,
                meshColliderPrefab != null ? meshColliderPrefab : null,
                attributes,
                result => OnMeshGenerated(id, mesh, result)
            );
        }

        private void OnMeshGenerated(MeshId id, Mesh mesh, MeshGenerationResult result)
        {
            if (result.Status != MeshGenerationStatus.Success) return;

            _meshesById[id] = mesh;

            if (!_meshGosById.TryGetValue(id, out var go) || go == null)
            {
                go = new GameObject($"SpatialMesh_{id}");
                go.transform.SetParent(transform, false);
                var mf = go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>(); // optional, material set in editor
                mf.sharedMesh = mesh;

                if (generateColliders)
                {
                    var mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mesh;
                }

                _meshGosById[id] = go;
            }
            else
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null) mf.sharedMesh = mesh;

                if (generateColliders)
                {
                    var mc = go.GetComponent<MeshCollider>();
                    if (mc != null) mc.sharedMesh = mesh;
                }
            }
        }

        public string ExportCurrentSnapshotAsObj(string filenamePrefix = "house-scan")
        {
            var outDir = Application.persistentDataPath;
            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var path = Path.Combine(outDir, $"{filenamePrefix}-{ts}.obj");

            var sb = new StringBuilder(1024 * 1024);
            sb.AppendLine("# BoreDOOM spatial mesh export");
            sb.AppendLine($"# generated_utc {DateTime.UtcNow:O}");

            int vertexOffset = 1;
            foreach (var kv in _meshesById)
            {
                var mesh = kv.Value;
                if (mesh == null) continue;

                sb.AppendLine($"o spatialmesh_{kv.Key}");

                var verts = mesh.vertices;
                for (int i = 0; i < verts.Length; i++)
                {
                    var v = verts[i];
                    sb.AppendLine($"v {v.x} {v.y} {v.z}");
                }

                var normals = mesh.normals;
                if (normals != null && normals.Length == verts.Length)
                {
                    for (int i = 0; i < normals.Length; i++)
                    {
                        var n = normals[i];
                        sb.AppendLine($"vn {n.x} {n.y} {n.z}");
                    }
                }

                var tris = mesh.triangles;
                for (int i = 0; i < tris.Length; i += 3)
                {
                    int a = tris[i + 0] + vertexOffset;
                    int b = tris[i + 1] + vertexOffset;
                    int c = tris[i + 2] + vertexOffset;

                    // If normals are present: use v//vn format with identical indexing.
                    if (normals != null && normals.Length == verts.Length)
                        sb.AppendLine($"f {a}//{a} {b}//{b} {c}//{c}");
                    else
                        sb.AppendLine($"f {a} {b} {c}");
                }

                vertexOffset += verts.Length;
            }

            File.WriteAllText(path, sb.ToString());
            return path;
        }
    }
}

