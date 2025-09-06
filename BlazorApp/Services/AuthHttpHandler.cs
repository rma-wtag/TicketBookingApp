using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;

namespace BlazorApp.Services
{
    public class AuthHttpHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public AuthHttpHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add CORS headers for WebAssembly
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Omit);
            request.SetBrowserRequestCache(BrowserRequestCache.NoCache);

            var token = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "accessToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
