using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "accessToken");

            if (string.IsNullOrEmpty(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                Console.WriteLine(user);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void MarkUserAsAuthenticated(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(jwt);
            var claims = jsonToken.Claims.ToList();

            var nameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "name");
            if (nameClaim != null)
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));

            // Map "role" claims to ClaimTypes.Role so IsInRole() works
            claims.AddRange(jsonToken.Claims
                .Where(c => c.Type == "role")
                .Select(c => new Claim(ClaimTypes.Role, c.Value)));


            return claims;
        }

    }
}
