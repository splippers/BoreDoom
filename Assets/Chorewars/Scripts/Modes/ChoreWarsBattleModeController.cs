using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Chorewars.Integration;

namespace Chorewars.Modes
{
    /// <summary>
    /// ChoreWars Battle: two players race the same chore in their own homes.
    /// Uses LocalNetworkAPI (UDP port 27877) to sync coverage progress in real-time.
    /// Each player sees the opponent's score as a ghost percentage overlay.
    ///
    /// Inherits coverage tracking from BaseCoverageModeController.
    /// </summary>
    public class ChoreWarsBattleModeController : BaseCoverageModeController
    {
        [Header("Battle HUD")]
        [SerializeField] private TMP_Text opponentLabel;      // "Opponent: 47%"
        [SerializeField] private TMP_Text battleStatusLabel;  // "YOU'RE WINNING" / "CATCH UP!"
        [SerializeField] private GameObject winBanner;
        [SerializeField] private GameObject loseBanner;

        [Header("Network")]
        [SerializeField] private LocalNetworkAPI networkAPI;
        [SerializeField] private float broadcastIntervalSeconds = 1.5f;

        private float _opponentCoverage;
        private float _localCoverage;
        private Coroutine _broadcastCoroutine;
        private bool _battleOver;

        protected override void OnModeBegun()
        {
            base.OnModeBegun();
            _opponentCoverage = 0f;
            _battleOver = false;

            if (winBanner  != null) winBanner.SetActive(false);
            if (loseBanner != null) loseBanner.SetActive(false);

            if (networkAPI != null)
            {
                networkAPI.OnCoverageBroadcastReceived += HandleOpponentUpdate;
                _broadcastCoroutine = StartCoroutine(BroadcastLoop());
            }
        }

        protected override void OnModeEnded(Core.ChoreResult result)
        {
            _battleOver = true;
            if (_broadcastCoroutine != null) StopCoroutine(_broadcastCoroutine);

            if (networkAPI != null)
                networkAPI.OnCoverageBroadcastReceived -= HandleOpponentUpdate;

            bool won = _localCoverage >= _opponentCoverage;
            if (winBanner  != null) winBanner.SetActive(won);
            if (loseBanner != null) loseBanner.SetActive(!won);

            base.OnModeEnded(result);
        }

        protected override void OnTrackedPositionSampled(Vector3 worldPos)
        {
            base.OnTrackedPositionSampled(worldPos);
            // _localCoverage is kept in sync by the base class HUD calls;
            // we read it back from the base result computation when needed.
        }

        protected override float GetBonusPoints()
        {
            // Winning bonus: up to 200 pts proportional to margin
            float margin = _localCoverage - _opponentCoverage;
            if (margin <= 0f) return 0f;
            return Mathf.Clamp(margin * 4f, 0f, 200f);
        }

        private void HandleOpponentUpdate(float opponentPct)
        {
            _opponentCoverage = opponentPct;

            if (opponentLabel != null)
                opponentLabel.text = $"Opponent: {opponentPct:F0}%";

            if (battleStatusLabel != null)
            {
                bool winning = _localCoverage > _opponentCoverage;
                battleStatusLabel.text = winning ? "YOU'RE WINNING!" : "CATCH UP!";
                battleStatusLabel.color = winning
                    ? new Color(0.3f, 1f, 0.3f)
                    : new Color(1f, 0.4f, 0.2f);
            }
        }

        private IEnumerator BroadcastLoop()
        {
            var wait = new WaitForSeconds(broadcastIntervalSeconds);
            while (!_battleOver)
            {
                networkAPI?.BroadcastCoverageUpdate(_localCoverage);
                yield return wait;
            }
        }

        // Called by base class HUD path — capture current coverage
        public void SetLocalCoverage(float pct) => _localCoverage = pct;
    }
}
