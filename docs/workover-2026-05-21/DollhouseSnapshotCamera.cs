using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Chorewars.Dollhouse
{
    /// <summary>
    /// Renders the dollhouse from directly above as a 1080×1080 share card and saves it to device.
    /// Attach to a Camera component positioned above the dollhouse, orthographic, facing down.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DollhouseSnapshotCamera : MonoBehaviour
    {
        [SerializeField] private Canvas overlayCanvas; // score, room labels, grade
        [SerializeField] private int resolution = 1080;

        private Camera _cam;

        private void Awake() => _cam = GetComponent<Camera>();

        public Texture2D Capture()
        {
            var rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
            _cam.targetTexture = rt;
            _cam.Render();
            _cam.targetTexture = null;

            RenderTexture.active = rt;
            var tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            Destroy(rt);

            Save(tex);
            return tex;
        }

        private void Save(Texture2D tex)
        {
            var dir = Application.persistentDataPath;
            var filename = $"boredoom-share-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png";
            var path = Path.Combine(dir, filename);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Debug.Log($"[Dollhouse] Share card saved: {path}");

#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, make the file visible in the gallery via MediaScanner
            using var mediaScannerClass = new AndroidJavaClass("android.media.MediaScannerConnection");
            using var unityActivity    = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                                              .GetStatic<AndroidJavaObject>("currentActivity");
            mediaScannerClass.CallStatic("scanFile", unityActivity, new[] { path }, null, null);
#endif
        }
    }
}
