using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Chorewars.UI
{
    /// <summary>
    /// In-session HUD: coverage %, elapsed time, current grade.
    /// Designed as a non-occluding overlay — attaches to the Quest passthrough layer.
    /// Assign UI references in the Inspector. All fields are optional; missing refs are silently skipped.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Coverage")]
        [SerializeField] private Slider coverageSlider;
        [SerializeField] private TMP_Text coverageLabel;

        [Header("Timer")]
        [SerializeField] private TMP_Text timerLabel;

        [Header("Grade")]
        [SerializeField] private TMP_Text gradeLabel;

        [Header("Style")]
        [SerializeField] private bool nonOccludingOverlay = true;

        private float _startTime;
        private bool _running;

        private void OnEnable()
        {
            _startTime = Time.unscaledTime;
            _running = true;
        }

        private void OnDisable() => _running = false;

        private void Update()
        {
            if (!_running) return;

            float elapsed = Time.unscaledTime - _startTime;
            int minutes = (int)(elapsed / 60f);
            int seconds = (int)(elapsed % 60f);
            if (timerLabel != null)
                timerLabel.text = $"{minutes:D2}:{seconds:D2}";
        }

        public void SetCoverage(float percent)
        {
            if (coverageSlider != null) coverageSlider.value = percent / 100f;
            if (coverageLabel != null)  coverageLabel.text  = $"{percent:F0}%";
            if (gradeLabel != null)     gradeLabel.text     = CoverageToGrade(percent);
        }

        private static string CoverageToGrade(float pct) => pct switch
        {
            >= 90f => "S",
            >= 75f => "A",
            >= 60f => "B",
            >= 40f => "C",
            _ => "D"
        };
    }
}
