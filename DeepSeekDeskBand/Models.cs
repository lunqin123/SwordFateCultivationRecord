namespace DeepSeekDeskBand;

public class BalanceInfo
{
    public double TotalBalance { get; set; }
    public double GrantedBalance { get; set; }
    public double ToppedUpBalance { get; set; }
    public string Currency { get; set; } = "CNY";
}

public class UsageRecord
{
    public string RequestId { get; set; } = "";
    public string Model { get; set; } = "";
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public string Timestamp { get; set; } = "";
}

public class PricingInfo
{
    public double InputPrice { get; set; }
    public double OutputPrice { get; set; }
    public double CacheHitPrice { get; set; }
}

public class UsageDayData
{
    public string Date { get; set; } = "";
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public double Cost { get; set; }
    public int Requests { get; set; }
    public long TotalTokens => PromptTokens + CompletionTokens;
}

public class ConsumptionDay
{
    public double Cost { get; set; }
    public long Tokens { get; set; }
    public int Events { get; set; }
}

// JSON deserialization types (System.Text.Json)
public class BalanceResponse
{
    public BalanceInfoItem[]? balance_infos { get; set; }
    public string? total_balance { get; set; }
    public string? granted_balance { get; set; }
    public string? topped_up_balance { get; set; }
}

public class BalanceInfoItem
{
    public string? currency { get; set; }
    public string? total_balance { get; set; }
    public string? granted_balance { get; set; }
    public string? topped_up_balance { get; set; }
}

public class UsageResponse
{
    public UsageItem[]? data { get; set; }
}

public class UsageItem
{
    public string? request_id { get; set; }
    public string? model { get; set; }
    public string? prompt_tokens { get; set; }
    public string? completion_tokens { get; set; }
    public string? timestamp { get; set; }
}
