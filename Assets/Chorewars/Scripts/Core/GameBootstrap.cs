using UnityEngine;
using Chorewars.Modes;

namespace Chorewars.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private ChoreModeRegistry registry;

        private IChoreMode _activeMode;

        public void StartModeById(string modeId)
        {
            if (registry == null) return;
            if (!registry.TryGet(modeId, out var descriptor)) return;
            if (descriptor == null || descriptor.modePrefab == null) return;

            var instance = Instantiate(descriptor.modePrefab);
            _activeMode = instance.GetComponent<IChoreMode>();
            _activeMode?.Begin();
        }

        public void EndActiveMode()
        {
            _activeMode?.End();
            _activeMode = null;
        }
    }
}
