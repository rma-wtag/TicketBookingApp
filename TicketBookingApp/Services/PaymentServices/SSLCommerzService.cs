using System.Text.Json;

namespace TicketBookingApp.Services.PaymentServices
{
    public class SSLCommerzService
    {
        private readonly HttpClient _http;
        private readonly string _storeId;
        private readonly string _storePass;

        public SSLCommerzService(HttpClient http,IConfiguration config)
        {
            _http = http;
            _storeId = config["SSLCOMMERZ:StoreId"]!;
            _storePass = config["SSLCOMMERZ:StorePass"]!;
        }

        private string BaseUrl => "https://sandbox.sslcommerz.com";

        public async Task<string?> CreateSessionAsync(Dictionary<string, string> payload, CancellationToken ct = default)
        {
            payload["store_id"] = _storeId;
            payload["store_passwd"] = _storePass;

            var form = new FormUrlEncodedContent(payload);
            var resp = await _http.PostAsync($"{BaseUrl}/gwprocess/v4/api.php", form, ct);

            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.TryGetProperty("GatewayPageURL", out var url)
                ? url.GetString()
                : null;
        }

        public async Task<JsonDocument?> ValidateAsync(string valId, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/validator/api/validationserverAPI.php?val_id={valId}&store_id={_storeId}&store_passwd={_storePass}&v=1&format=json";
            var resp = await _http.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonDocument.Parse(json);
        }
    }
}