using System;
using UnityEngine;

namespace NitroRush.Multiplayer
{
    [Serializable]
    public class LocalInputPacket
    {
        public uint seq;
        public float throttle;
        public float steer;
        public bool nitro;
        public bool drift;
    }

    /// <summary>
    /// Sends local inputs at fixed 20Hz and receives server position authority.
    /// Note: Server-authoritative design - client NEVER claims currency or rewards locally.
    /// </summary>
    public class RaceNetworkSync : MonoBehaviour
    {
        [SerializeField] private float sendInterval = 0.05f; // 20Hz
        private float _sendTimer = 0f;
        private uint _sequenceCounter = 0;
        private WebSocketClient _webSocketClient;

        public void Initialize(WebSocketClient wsClient)
        {
            _webSocketClient = wsClient;
        }

        private void Update()
        {
            _sendTimer += Time.deltaTime;
            if (_sendTimer >= sendInterval)
            {
                _sendTimer -= sendInterval;
                SendLocalInput();
            }
        }

        private void SendLocalInput()
        {
            if (_webSocketClient == null || _webSocketClient.State != ConnectionState.Connected)
                return;

            _sequenceCounter++;
            LocalInputPacket packet = new LocalInputPacket
            {
                seq = _sequenceCounter,
                throttle = Input.GetAxis("Vertical"),
                steer = Input.GetAxis("Horizontal"),
                nitro = Input.GetKey(KeyCode.LeftShift),
                drift = Input.GetKey(KeyCode.Space)
            };

            string json = JsonUtility.ToJson(packet);
            _ = _webSocketClient.SendMessageAsync("input_packet", json);
        }
    }
}
