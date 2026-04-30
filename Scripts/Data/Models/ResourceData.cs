namespace SwordFateCultivationRecord;

public class ResourceData
{
    public Dictionary<ResourceType, int> Amounts { get; set; } = new();
    public Dictionary<ResourceType, int> IncomePerDay { get; set; } = new();

    public int Get(ResourceType type) => Amounts.GetValueOrDefault(type, 0);

    public void Add(ResourceType type, int amount)
    {
        Amounts.TryAdd(type, 0);
        Amounts[type] += amount;
    }

    public bool Spend(ResourceType type, int amount)
    {
        if (Get(type) < amount) return false;
        Amounts[type] -= amount;
        return true;
    }

    public void ClearIncome()
    {
        IncomePerDay.Clear();
    }
}
