using System.Text.Json;

namespace DeepSeekDeskBand;

public static class ApiClient
{
    private const string BalanceUrl = "https://api.deepseek.com/user/balance";
    private const string UsageUrl = "https://api.deepseek.com/v1/usage";
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static string _apiKey = "";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static void SetApiKey(string key)
    {
        _apiKey = key;
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
    }

    public static async Task<BalanceInfo?> FetchBalanceAsync()
    {
        if (string.IsNullOrEmpty(_apiKey)) return null;
        try
        {
            var json = await Client.GetStringAsync(BalanceUrl);
            var resp = JsonSerializer.Deserialize<BalanceResponse>(json, JsonOpts);
            if (resp == null) return null;

            var info = new BalanceInfo();
            if (resp.balance_infos is { Length: > 0 })
            {
                foreach (var bi in resp.balance_infos)
                {
                    info.Currency = bi.currency ?? "CNY";
                    info.TotalBalance += ParseD(bi.total_balance);
                    info.GrantedBalance += ParseD(bi.granted_balance);
                    info.ToppedUpBalance += ParseD(bi.topped_up_balance);
                }
            }
            else
            {
                info.TotalBalance = ParseD(resp.total_balance);
                info.GrantedBalance = ParseD(resp.granted_balance);
                info.ToppedUpBalance = ParseD(resp.topped_up_balance);
            }
            return info;
        }
        catch { return null; }
    }

    public static async Task<List<UsageRecord>> FetchUsageAsync(DateTime start, DateTime end)
    {
        if (string.IsNullOrEmpty(_apiKey)) return [];
        try
        {
            var url = $"{UsageUrl}?start_date={start:yyyy-MM-dd}&end_date={end:yyyy-MM-dd}";
            var json = await Client.GetStringAsync(url);
            var resp = JsonSerializer.Deserialize<UsageResponse>(json, JsonOpts);
            if (resp?.data == null) return [];

            return resp.data.Select(item => new UsageRecord
            {
                RequestId = item.request_id ?? "",
                Model = item.model ?? "unknown",
                PromptTokens = ParseI(item.prompt_tokens),
                CompletionTokens = ParseI(item.completion_tokens),
                Timestamp = item.timestamp ?? "",
            }).ToList();
        }
        catch { return []; }
    }

    public static PricingInfo GetPricing(string model)
    {
        var m = model?.ToLowerInvariant() ?? "";
        if (m.Contains("v4-pro") || m.Contains("reasoner"))
            return new() { InputPrice = 3.0, OutputPrice = 6.0, CacheHitPrice = 0.025 };
        return new() { InputPrice = 1.0, OutputPrice = 2.0, CacheHitPrice = 0.02 };
    }

    private static double ParseD(string? s)
    {
        if (double.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v)) return v;
        return 0;
    }

    private static int ParseI(string? s)
    {
        if (int.TryParse(s, out var v)) return v;
        return 0;
    }
}
