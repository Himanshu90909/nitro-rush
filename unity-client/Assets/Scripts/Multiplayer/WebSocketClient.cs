using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NitroRush.Multiplayer
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting
    }

    [Serializable]
    public class WsMessageEnvelope
    {
        public string type;
        public string payload;
    }

    /// <summary>
    /// Manages ClientWebSocket connection with async receive loop and exponential backoff reconnects.
    /// </summary>
    public class WebSocketClient
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        private string _serverUrl;

        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
        public event Action<string, string> OnMessage;
        public event Action<ConnectionState> OnStateChanged;

        public async Task ConnectAsync(string serverUrl)
        {
            _serverUrl = serverUrl;
            SetState(ConnectionState.Connecting);

            _webSocket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            try
            {
                await _webSocket.ConnectAsync(new Uri(_serverUrl), _cts.Token);
                SetState(ConnectionState.Connected);
                _ = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebSocket connection failed: {ex.Message}");
                _ = AttemptReconnectAsync();
            }
        }

        public async Task SendMessageAsync(string type, string payload)
        {
            if (State != ConnectionState.Connected || _webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                Debug.LogWarning("Cannot send message, WebSocket is not open.");
                return;
            }

            var envelope = new WsMessageEnvelope { type = type, payload = payload };
            string json = JsonUtility.ToJson(envelope);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ReceiveLoopAsync()
        {
            byte[] buffer = new byte[4096];

            while (_webSocket != null && _webSocket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        SetState(ConnectionState.Disconnected);
                        break;
                    }

                    string jsonText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    WsMessageEnvelope envelope = JsonUtility.FromJson<WsMessageEnvelope>(jsonText);
                    if (envelope != null)
                    {
                        OnMessage?.Invoke(envelope.type, envelope.payload);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"WebSocket receive error: {ex.Message}");
                    _ = AttemptReconnectAsync();
                    break;
                }
            }
        }

        private async Task AttemptReconnectAsync()
        {
            SetState(ConnectionState.Reconnecting);
            int retryAttempt = 0;
            int maxDelay = 16000; // 16s max

            while (State == ConnectionState.Reconnecting && retryAttempt < 5)
            {
                retryAttempt++;
                int delayMs = Mathf.Min((int)Mathf.Pow(2, retryAttempt) * 1000, maxDelay);
                await Task.Delay(delayMs);

                try
                {
                    _webSocket?.Dispose();
                    _webSocket = new ClientWebSocket();
                    _cts = new CancellationTokenSource();
                    await _webSocket.ConnectAsync(new Uri(_serverUrl), _cts.Token);
                    SetState(ConnectionState.Connected);
                    _ = ReceiveLoopAsync();
                    return;
                }
                catch
                {
                    Debug.LogWarning($"Reconnect attempt {retryAttempt} failed.");
                }
            }

            SetState(ConnectionState.Disconnected);
        }

        public async Task DisconnectAsync()
        {
            _cts?.Cancel();
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User disconnected", CancellationToken.None);
            }
            SetState(ConnectionState.Disconnected);
        }

        private void SetState(ConnectionState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(State);
        }
    }
}
