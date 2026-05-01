using UnityEngine;

namespace Chorewars.Modes
{
    [CreateAssetMenu(menuName = "Chorewars/Chore Mode Descriptor")]
    public class ChoreModeDescriptor : ScriptableObject
    {
        public string modeId;
        public string displayName;
        public GameObject modePrefab;
    }
}
