using UnityEngine;

namespace Chorewars.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private bool nonOccludingOverlay = true;

        private void Awake()
        {
            _ = nonOccludingOverlay;
        }
    }
}
