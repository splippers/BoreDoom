using UnityEngine;

namespace Chorewars.Core
{
    public enum ChoreType
    {
        Hoovering,
        Mowing
    }

    [CreateAssetMenu(menuName = "Chorewars/Chore Definition")]
    public class ChoreDefinition : ScriptableObject
    {
        public string choreId;
        public string displayName;
        public ChoreType type;
        public float recommendedDurationMinutes = 10f;
        [Range(0f, 100f)] public float targetCoveragePercent = 80f;
    }
}
