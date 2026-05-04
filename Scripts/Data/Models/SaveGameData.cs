namespace SwordFateCultivationRecord;

public class SaveGameData
{
    public string SaveVersion { get; set; } = "1.0";
    public string SaveName { get; set; } = "";
    public DateTime SaveTime { get; set; }
    public double PlayTimeHours { get; set; }

    // Time
    public int CurrentYear { get; set; } = 1;
    public int CurrentMonth { get; set; } = 1;
    public int CurrentDay { get; set; } = 1;

    // Sect info
    public string SectName { get; set; } = "无名剑宗";
    public int SectLevel { get; set; } = 1;
    public int SectReputation { get; set; }
    public int SectPower { get; set; }
    public int MaxDisciples { get; set; } = 5;
    public int OuterDiscipleCount { get; set; }
    public int OuterGatherRatio { get; set; } = 60;
    public int OuterTradeRatio { get; set; } = 20;
    public double TradeAccum { get; set; }
    public double HerbAccum { get; set; }
    public double OreAccum { get; set; }
    public double OuterGrowthAccum { get; set; }

    // State
    public Dictionary<ResourceType, int> Resources { get; set; } = new();
    public Dictionary<ResourceType, int> IncomePerDay { get; set; } = new();
    public List<DiscipleData> Disciples { get; set; } = new();
    public List<FacilityData> Facilities { get; set; } = new();
    public List<LogEntry> EventLog { get; set; } = new();
    public Dictionary<int, int> EventCooldowns { get; set; } = new();
    public List<CompanionData> Companions { get; set; } = new();
    public List<SectQuestData> Quests { get; set; } = new();
    public List<EquipmentData> Equipment { get; set; } = new();
    public PlotProgress PlotProgress { get; set; } = new();
    public AchievementProgress AchievementProgress { get; set; } = new();
    public int RngSeed { get; set; }
}

public class LogEntry
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public int Day { get; set; }
}
