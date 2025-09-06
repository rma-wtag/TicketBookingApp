using BlazorApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(UserLoginDTO loginDto);
        Task<bool> RegisterAsync(UserRegisterDTO registerDto);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetTokenAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> LoginAsync(UserLoginDTO loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
                    if (authResponse != null)
                    {
                        // Store tokens in sessionStorage
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "accessToken", authResponse.AccessToken);
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "refreshToken", authResponse.RefreshToken);
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "tokenExpiry", authResponse.AccessTokenExpiresAt.ToString("O"));

                        // Update authentication state
                        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(authResponse.AccessToken);

                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(UserRegisterDTO registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", registerDto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "accessToken");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "tokenExpiry");

            ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "accessToken");
        }
    }
}
