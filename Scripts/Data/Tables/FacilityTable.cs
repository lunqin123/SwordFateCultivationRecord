namespace SwordFateCultivationRecord;

public static class FacilityTable
{
    public record FacilityInfo(
        string Name,
        string Description,
        int BaseBuildCost,
        int UpgradeCostPerLevel,
        int BaseOutput,
        ResourceType OutputType,
        int MaxDisciples,
        int BuildDays,
        int MinSectLevel = 1     // 最低宗门等级要求
    );

    private static readonly Dictionary<FacilityType, FacilityInfo> _facilities = new()
    {
        [FacilityType.MeditationChamber] = new("静修室", "提升弟子修炼速度", 100, 80, 5, ResourceType.SpiritEssence, 3, 7),
        [FacilityType.AlchemyRoom] = new("丹房", "可炼制丹药", 200, 150, 2, ResourceType.Pill, 2, 15, 2),
        [FacilityType.TrainingGround] = new("演武场", "提升弟子战斗能力", 150, 100, 3, ResourceType.SpiritEssence, 5, 10),
        [FacilityType.Library] = new("藏经阁", "增加弟子悟性", 300, 200, 1, ResourceType.SpiritEssence, 3, 20, 2),
        [FacilityType.PillRefinery] = new("炼药房", "灵草加工为丹药", 150, 100, 3, ResourceType.Pill, 2, 12),
        [FacilityType.SpiritGarden] = new("灵田", "种植灵草", 80, 60, 5, ResourceType.Herb, 3, 5),
        [FacilityType.OreMine] = new("矿脉", "开采矿石", 120, 90, 4, ResourceType.Ore, 3, 8),
        [FacilityType.FormationHall] = new("阵法殿", "提升宗门防御", 400, 300, 1, ResourceType.SpiritEssence, 2, 25, 3),
        [FacilityType.DiningHall] = new("膳堂", "提升弟子心情", 60, 40, 10, ResourceType.SpiritEssence, 1, 5),
        [FacilityType.GuestHall] = new("会客厅", "吸引散人来投", 200, 120, 2, ResourceType.SpiritEssence, 2, 12, 2),
    };

    public static IReadOnlyDictionary<FacilityType, FacilityInfo> GetAll() => _facilities;

    public static FacilityInfo GetInfo(FacilityType type) => _facilities[type];

    public static int GetBuildCost(FacilityType type) => _facilities[type].BaseBuildCost;

    public static int GetUpgradeCost(FacilityType type, int level)
    {
        var info = _facilities[type];
        return info.UpgradeCostPerLevel * level;
    }

    public static int GetOutput(FacilityType type, int level) =>
        _facilities[type].BaseOutput * level;

    public static int GetMaxLevel() => 10;

    // ====== Facility-Task synergy ======
    // Each facility type boosts specific disciple tasks when the facility is built.
    private static readonly Dictionary<FacilityType, DiscipleTaskType> _taskSynergy = new()
    {
        [FacilityType.MeditationChamber] = DiscipleTaskType.Cultivate,
        [FacilityType.AlchemyRoom] = DiscipleTaskType.Alchemy,
        [FacilityType.TrainingGround] = DiscipleTaskType.Train,
        [FacilityType.Library] = DiscipleTaskType.Teach,
        [FacilityType.PillRefinery] = DiscipleTaskType.Alchemy,
        [FacilityType.SpiritGarden] = DiscipleTaskType.Gather,
        [FacilityType.OreMine] = DiscipleTaskType.Gather,
        [FacilityType.FormationHall] = DiscipleTaskType.Guard,
        [FacilityType.DiningHall] = DiscipleTaskType.Rest,
        [FacilityType.GuestHall] = DiscipleTaskType.Cultivate, // guest experts share insights
    };

    /// <summary>Returns the per-level bonus multiplier for a facility type (0.15 per level).</summary>
    public static double GetTaskBonus(FacilityType type, int level, DiscipleTaskType task)
    {
        if (!_taskSynergy.TryGetValue(type, out var boostedTask)) return 0;
        if (boostedTask != task) return 0;
        return 0.15 * level; // +15% per level, Lv.5 = +75%, Lv.10 = +150%
    }

    /// <summary>Get the total bonus for a task from all built facilities.</summary>
    public static double GetTotalTaskBonus(DiscipleTaskType task, IEnumerable<FacilityData> facilities)
    {
        double total = 0;
        foreach (var f in facilities.Where(f => f.IsBuilt))
            total += GetTaskBonus(f.Type, f.Level, task);
        return total;
    }
}
