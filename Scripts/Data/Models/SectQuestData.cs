namespace SwordFateCultivationRecord;

public enum QuestType
{
    RecruitDisciples,    // 招募内门弟子
    BuildFacilities,     // 建造设施
    ReachReputation,     // 达到声望
    GatherResource,      // 收集资源
    DiscipleBreakthrough,// 弟子突破
    UpgradeFacilities,   // 升级设施
    AccumulateEquipment, // 积累法器
    OuterDiscipleCount,  // 外门壮大
    SectPowerReach,      // 战力达标
}

public class SectQuestData
{
    public int Id { get; set; }
    public QuestType Type { get; set; }
    public int Tier { get; set; } = 1; // 1=凡品 2=良品 3=优品 4=极品
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int TargetCount { get; set; }
    public int CurrentCount { get; set; }
    public bool Completed { get; set; }

    public string TierLabel => Tier switch { 1 => "凡品", 2 => "良品", 3 => "优品", 4 => "极品", _ => "?" };
    public Color TierColor => Tier switch { 1 => new Color(0.7f, 0.7f, 0.7f), 2 => new Color(0.3f, 0.8f, 0.3f), 3 => new Color(0.3f, 0.5f, 1.0f), 4 => new Color(0.9f, 0.5f, 0.0f), _ => new Color(0.7f, 0.7f, 0.7f) };

    // Rewards
    public int RewardReputation { get; set; }
    public Dictionary<ResourceType, int> RewardResources { get; set; } = new();
    public int RewardEquipment { get; set; } // number of equipment items

    public string ProgressText => Completed ? "✓ 已完成" : $"{CurrentCount}/{TargetCount}";

    public string RewardText
    {
        get
        {
            var parts = new List<string>();
            if (RewardReputation > 0) parts.Add($"声望+{RewardReputation}");
            foreach (var kv in RewardResources)
                if (kv.Value > 0) parts.Add($"{ResName(kv.Key)}+{kv.Value}");
            if (RewardEquipment > 0) parts.Add($"法器×{RewardEquipment}");
            return string.Join("  ", parts);
        }
    }

    static string ResName(ResourceType rt) => rt switch
    {
        ResourceType.SpiritStone => "灵石", ResourceType.Herb => "灵草", ResourceType.Ore => "矿石",
        ResourceType.Pill => "丹药", ResourceType.SpiritEssence => "灵气", _ => rt.ToString()
    };
}
