using System;
using System.IO;
using UnityEngine;

namespace Chorewars.AR
{
    public sealed class ProjectedTextureBaker : MonoBehaviour
    {
        private static ProjectedTextureBaker _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance != null) return;
            var go = new GameObject(nameof(ProjectedTextureBaker));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ProjectedTextureBaker>();
        }

        [SerializeField] private SpatialMeshTracker meshTracker;
        [SerializeField] private Camera captureCamera;
        [SerializeField] private Shader projectedShader;

        private Material _material;

        private void Awake()
        {
            if (meshTracker == null) meshTracker = FindAnyObjectByType<SpatialMeshTracker>();
            if (captureCamera == null) captureCamera = Camera.main;
        }

        public bool TryBakeOnce(out string pngPath)
        {
            pngPath = null;

            if (meshTracker == null) meshTracker = FindAnyObjectByType<SpatialMeshTracker>();
            if (captureCamera == null) captureCamera = Camera.main;
            if (captureCamera == null) return false;

            if (projectedShader == null) projectedShader = Shader.Find("Chorewars/ProjectedTexture");
            if (projectedShader == null) return false;

            if (_material == null) _material = new Material(projectedShader);

            // Capture a composited frame (Quest will include passthrough in the final image).
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            if (tex == null) return false;

            // Save the frame so you can pull it later (useful for debugging / future multi-frame blending).
            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            pngPath = Path.Combine(Application.persistentDataPath, $"projected-frame-{ts}.png");
            try
            {
                File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            }
            catch
            {
                // non-fatal; projection can still work
                pngPath = null;
            }

            _material.SetTexture("_MainTex", tex);
            _material.SetMatrix("_ProjectVP", captureCamera.projectionMatrix * captureCamera.worldToCameraMatrix);

            ApplyMaterialToSpatialMeshes(_material);
            return true;
        }

        private void ApplyMaterialToSpatialMeshes(Material mat)
        {
            // Mesh GameObjects are created under meshParent if configured; we can safely re-skin all MeshRenderers.
            var renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                if (r.gameObject.name.StartsWith("SpatialMesh_", StringComparison.Ordinal))
                    r.sharedMaterial = mat;
            }
        }
    }
}

