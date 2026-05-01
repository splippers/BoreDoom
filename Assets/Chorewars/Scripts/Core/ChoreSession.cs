using System;
using UnityEngine;

namespace Chorewars.Core
{
    [Serializable]
    public class ChoreSession
    {
        public string sessionId;
        public ChoreDefinition chore;

        public DateTime startTimeUtc;
        public DateTime? endTimeUtc;

        [Range(0f, 100f)] public float coveragePercent;
        public float efficiencyScore;
        public float movementScore;
    }
}
