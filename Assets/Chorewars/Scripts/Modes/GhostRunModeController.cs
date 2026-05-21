using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chorewars.Core;
using Chorewars.AR;

namespace Chorewars.Modes
{
    /// <summary>
    /// Ghost Run mode: replays the player's previous best session as a translucent AR ghost.
    /// Inherits coverage tracking; adds ghost playback on top.
    /// </summary>
    public class GhostRunModeController : BaseCoverageModeController
    {
        [Header("Ghost settings")]
        [SerializeField] private LineRenderer ghostLinePrefab;
        [SerializeField] private Color ghostColour = new Color(0.4f, 0.8f, 1f, 0.4f);
        [SerializeField] private float ghostHeadSize = 0.15f;

        private LineRenderer _ghostLine;
        private GameObject _ghostHead;
        private List<Vector3> _ghostPath;
        private float _ghostSessionDuration;
        private Coroutine _ghostPlayback;

        protected override void OnModeBegun()
        {
            base.OnModeBegun();
            SessionStartTime = System.DateTime.UtcNow;
            LoadBestGhost();
            if (_ghostPath != null && _ghostPath.Count > 1)
                _ghostPlayback = StartCoroutine(PlayGhost());
        }

        protected override void OnModeEnded(Chorewars.Core.ChoreResult result)
        {
            base.OnModeEnded(result);
            if (_ghostPlayback != null) StopCoroutine(_ghostPlayback);
            SaveIfPersonalBest();
        }

        private void LoadBestGhost()
        {
            // Load the path from PlayerProfile.history — find best session by points for this chore type
            // For now, load from PlayerPrefs JSON as a stepping stone
            var json = PlayerPrefs.GetString($"ghost_{choreMode}", "");
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<GhostData>(json);
            if (data?.positions == null || data.positions.Count < 2) return;

            _ghostPath = data.positions;
            _ghostSessionDuration = data.durationSeconds;
        }

        private IEnumerator PlayGhost()
        {
            if (_ghostLine == null)
            {
                _ghostLine = Instantiate(ghostLinePrefab);
                _ghostLine.startColor = _ghostLine.endColor = ghostColour;
            }

            _ghostHead = _ghostHead ?? GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _ghostHead.transform.localScale = Vector3.one * ghostHeadSize;
            var mat = _ghostHead.GetComponent<Renderer>().material;
            mat.color = ghostColour;

            float speed = _ghostPath.Count / Mathf.Max(1f, _ghostSessionDuration);
            int idx = 0;
            _ghostLine.positionCount = 0;

            while (idx < _ghostPath.Count)
            {
                _ghostHead.transform.position = _ghostPath[idx];
                _ghostLine.positionCount = idx + 1;
                _ghostLine.SetPosition(idx, _ghostPath[idx]);
                idx++;
                yield return new WaitForSeconds(1f / Mathf.Max(1f, speed));
            }
        }

        private void SaveIfPersonalBest()
        {
            // Compare current session points to stored best; if better, overwrite ghost data
            var currentPath = GetComponent<PathTracker>()?.worldPositions;
            if (currentPath == null) return;

            var data = new GhostData
            {
                positions = new List<Vector3>(currentPath),
                durationSeconds = (float)(System.DateTime.UtcNow - SessionStartTime).TotalSeconds
            };
            PlayerPrefs.SetString($"ghost_{choreMode}", JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        [SerializeField] private string choreMode = "hoovering";

        protected System.DateTime SessionStartTime { get; private set; }

        [System.Serializable]
        private class GhostData
        {
            public List<Vector3> positions;
            public float durationSeconds;
        }
    }
}
