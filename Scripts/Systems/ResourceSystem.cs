namespace SwordFateCultivationRecord;

public class ResourceSystem
{
    private readonly Dictionary<ResourceType, int> _resources = new()
    {
        [ResourceType.SpiritStone] = 500,
        [ResourceType.Herb] = 20,
        [ResourceType.Ore] = 20,
        [ResourceType.Pill] = 5,
        [ResourceType.Equipment] = 0,
        [ResourceType.Contribution] = 0,
        [ResourceType.SpiritEssence] = 50,
    };

    private readonly Dictionary<ResourceType, int> _dailyIncome = new();

    public int Get(ResourceType type) => _resources.GetValueOrDefault(type, 0);

    public Dictionary<ResourceType, int> GetAllResources() => new(_resources);

    public int GetIncome(ResourceType type) => _dailyIncome.GetValueOrDefault(type, 0);

    public Dictionary<ResourceType, int> GetAllIncome() => new(_dailyIncome);

    public bool CanAfford(ResourceType type, int amount) => Get(type) >= amount;

    public bool Spend(ResourceType type, int amount)
    {
        if (amount < 0) { Add(type, -amount); return true; }
        if (!CanAfford(type, amount)) return false;
        int old = _resources[type];
        _resources[type] -= amount;
        EventBus.EmitResourceChanged(type, old, _resources[type]);
        return true;
    }

    public void Add(ResourceType type, int amount)
    {
        if (amount == 0) return;
        _resources.TryAdd(type, 0);
        int old = _resources[type];
        _resources[type] += amount;
        EventBus.EmitResourceChanged(type, old, _resources[type]);
    }

    public void Set(ResourceType type, int amount)
    {
        _resources.TryAdd(type, 0);
        int old = _resources[type];
        _resources[type] = amount;
        EventBus.EmitResourceChanged(type, old, _resources[type]);
    }

    public void SetDailyIncome(Dictionary<ResourceType, int> income)
    {
        _dailyIncome.Clear();
        foreach (var kv in income)
            _dailyIncome[kv.Key] = kv.Value;
    }

    public void ApplyDailyIncome()
    {
        foreach (var kv in _dailyIncome)
        {
            if (kv.Value > 0)
                Add(kv.Key, kv.Value);
        }
    }

    public void ClearDailyIncome() => _dailyIncome.Clear();

    public void LoadState(Dictionary<ResourceType, int> resources, Dictionary<ResourceType, int> income)
    {
        _resources.Clear();
        foreach (var kv in resources) _resources[kv.Key] = kv.Value;
        _dailyIncome.Clear();
        foreach (var kv in income) _dailyIncome[kv.Key] = kv.Value;
    }
}
