using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NitroRush.Networking
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public ApiException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Wrapper for JSON array serialization in Unity JsonUtility.
    /// </summary>
    [Serializable]
    public class JsonArrayWrapper<T>
    {
        public T[] items;
    }

    /// <summary>
    /// Async REST client over UnityWebRequest with JWT bearer storage, timeouts, and retries.
    /// </summary>
    public class ApiClient
    {
        private readonly ApiConfig _config;

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public ApiClient(ApiConfig config)
        {
            _config = config ?? ScriptableObject.CreateInstance<ApiConfig>();
        }

        public async Task<T> GetAsync<T>(string path)
        {
            return await RequestAsync<T>(UnityWebRequest.kHttpVerbGET, path, null);
        }

        public async Task<T> PostAsync<T>(string path, object body)
        {
            string jsonBody = body != null ? JsonUtility.ToJson(body) : "";
            return await RequestAsync<T>(UnityWebRequest.kHttpVerbPOST, path, jsonBody);
        }

        private async Task<T> RequestAsync<T>(string method, string path, string jsonBody)
        {
            string fullUrl = _config.ApiBaseUrl + path;
            int attempts = 0;
            int maxRetries = Mathf.Max(1, _config.MaxRetries);

            while (attempts < maxRetries)
            {
                attempts++;
                using (var request = new UnityWebRequest(fullUrl, method))
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                    if (!string.IsNullOrEmpty(jsonBody))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        request.SetRequestHeader("Content-Type", "application/json");
                    }

                    if (!string.IsNullOrEmpty(AccessToken))
                    {
                        request.SetRequestHeader("Authorization", "Bearer " + AccessToken);
                    }

                    request.timeout = (int)_config.RequestTimeoutSec;

                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Delay(50);
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string responseText = request.downloadHandler.text;
                        if (typeof(T) == typeof(string))
                        {
                            return (T)(object)responseText;
                        }
                        return JsonUtility.FromJson<T>(responseText);
                    }

                    if (request.responseCode >= 400 && request.responseCode < 500)
                    {
                        throw new ApiException((int)request.responseCode, request.error + " | " + request.downloadHandler.text);
                    }

                    if (attempts >= maxRetries)
                    {
                        throw new ApiException((int)request.responseCode, request.error ?? "Network request failed after retries.");
                    }

                    await Task.Delay(1000 * attempts); // Exponential backoff delay
                }
            }

            throw new ApiException(500, "Request failed.");
        }
    }
}
