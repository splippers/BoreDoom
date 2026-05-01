using UnityEngine;
using Chorewars.Modes;

namespace Chorewars.UI
{
    public class ChoreSelectionUI : MonoBehaviour
    {
        [SerializeField] private ChoreModeRegistry registry;

        public void SelectMode(string modeId)
        {
            if (registry == null) return;
            if (!registry.TryGet(modeId, out var descriptor)) return;
            _ = descriptor;
        }
    }
}
