using System;
using System.Threading.Tasks;

namespace NitroRush.Networking
{
    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class RefreshRequest
    {
        public string refreshToken;
    }

    [Serializable]
    public class UserDto
    {
        public string id;
        public string username;
        public string email;
    }

    [Serializable]
    public class AuthResponse
    {
        public string accessToken;
        public string refreshToken;
        public UserDto user;
    }

    /// <summary>
    /// Authentication service API wrappers for login, registration, and token refresh.
    /// </summary>
    public class AuthApi
    {
        private readonly ApiClient _apiClient;

        public AuthApi(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var response = await _apiClient.PostAsync<AuthResponse>(ApiEndpoints.AuthRegister, request);
            if (response != null)
            {
                _apiClient.AccessToken = response.accessToken;
                _apiClient.RefreshToken = response.refreshToken;
            }
            return response;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var response = await _apiClient.PostAsync<AuthResponse>(ApiEndpoints.AuthLogin, request);
            if (response != null)
            {
                _apiClient.AccessToken = response.accessToken;
                _apiClient.RefreshToken = response.refreshToken;
            }
            return response;
        }

        public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
        {
            var response = await _apiClient.PostAsync<AuthResponse>(ApiEndpoints.AuthRefresh, request);
            if (response != null)
            {
                _apiClient.AccessToken = response.accessToken;
                _apiClient.RefreshToken = response.refreshToken;
            }
            return response;
        }
    }
}
