using System.Text.Json;

namespace DeepSeekDeskBand;

public static class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, ".deepseek_config.json");

    private static ConfigData? _cache;
    private static readonly Dictionary<string, ConsumptionDay> Consumption = [];

    public class ConfigData
    {
        public string ApiKey { get; set; } = "";
        public int RefreshInterval { get; set; } = 30;
        public double LastBalanceTotal { get; set; }
    }

    public static string LoadApiKey()
    {
        return Load().ApiKey;
    }

    public static int LoadInterval()
    {
        return Math.Clamp(Load().RefreshInterval, 10, 3600);
    }

    public static ConfigData Load()
    {
        if (_cache != null) return _cache;
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _cache = JsonSerializer.Deserialize<ConfigData>(json) ?? new();
                return _cache;
            }
        }
        catch { }
        _cache = new();
        return _cache;
    }

    public static void Save(ConfigData cfg)
    {
        _cache = cfg;
        try
        {
            var json = JsonSerializer.Serialize(cfg);
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public static void SaveApiKey(string key, int interval = 30)
    {
        var cfg = Load();
        cfg.ApiKey = key;
        cfg.RefreshInterval = interval;
        Save(cfg);
    }

    public static double GetLastBalance() => Load().LastBalanceTotal;

    public static void SaveLastBalance(double total)
    {
        var cfg = Load();
        cfg.LastBalanceTotal = total;
        Save(cfg);
    }

    public static void AddConsumption(string day, double cost, long tokens)
    {
        if (!Consumption.TryGetValue(day, out var c))
            Consumption[day] = c = new();
        c.Cost += cost;
        c.Tokens += tokens;
        c.Events++;
    }

    public static ConsumptionDay? GetTodayConsumption()
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        Consumption.TryGetValue(today, out var c);
        return c;
    }
}
