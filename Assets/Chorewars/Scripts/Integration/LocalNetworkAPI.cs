using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Chorewars.Integration
{
    public class LocalNetworkAPI : MonoBehaviour
    {
        [Serializable]
        public class AgentMessage
        {
            public string agentId;
            public string messageType;
            public string payloadJson;
        }

        [Header("Discovery (UDP)")]
        [SerializeField] private int discoveryPort = 27877;
        [SerializeField] private string discoveryMessage = "CHOREWARS_BOREDOOM_DISCOVERY_V1";

        private UdpClient _udp;

        private void OnEnable()
        {
            try
            {
                _udp = new UdpClient();
                _udp.EnableBroadcast = true;
            }
            catch (Exception)
            {
                _udp = null;
            }
        }

        private void OnDisable()
        {
            _udp?.Close();
            _udp = null;
        }

        public void BroadcastDiscovery()
        {
            if (_udp == null) return;
            byte[] bytes = Encoding.UTF8.GetBytes(discoveryMessage);
            _udp.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, discoveryPort));
        }

        public void HandleIncomingMessageJson(string json)
        {
            // Placeholder hook point: deserialize and route into HUD / challenge system.
            _ = json;
        }
    }
}
