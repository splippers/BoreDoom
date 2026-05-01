using System.Collections.Generic;
using UnityEngine;

namespace Chorewars.Modes
{
    public class ChoreModeRegistry : MonoBehaviour
    {
        [SerializeField] private List<ChoreModeDescriptor> modes = new();

        private readonly Dictionary<string, ChoreModeDescriptor> _byId = new();

        public IReadOnlyList<ChoreModeDescriptor> Modes => modes;

        private void Awake()
        {
            _byId.Clear();
            foreach (var mode in modes)
            {
                if (mode == null) continue;
                if (string.IsNullOrWhiteSpace(mode.modeId)) continue;
                _byId[mode.modeId] = mode;
            }
        }

        public bool TryGet(string modeId, out ChoreModeDescriptor descriptor) =>
            _byId.TryGetValue(modeId, out descriptor);
    }
}
