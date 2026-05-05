namespace SwordFateCultivationRecord;

public enum EventCategory
{
    Opportunity,   // 机缘
    Crisis,        // 危机
    Visitor,       // 来客
    Internal,      // 内部事务
    Competition    // 比武大会
}

public class EventData
{
    public int Id { get; set; }
    public EventCategory Category { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    public string Choice1Text { get; set; } = "";
    public string Choice2Text { get; set; } = "";
    public string Choice3Text { get; set; } = "";

    public EventOutcome Choice1Outcome { get; set; } = new();
    public EventOutcome Choice2Outcome { get; set; } = new();
    public EventOutcome Choice3Outcome { get; set; } = new();

    public int Weight { get; set; } = 10;
    public int CooldownDays { get; set; }
    public int MinSectLevel { get; set; }
}

public class DiscipleStatEffect
{
    public int LoyaltyChange { get; set; }
    public int MoodChange { get; set; }
    public int HealthChange { get; set; }
}

public class EventOutcome
{
    public string ResultText { get; set; } = "";
    public Dictionary<ResourceType, int> ResourceChanges { get; set; } = new();
    public int DiscipleCountChange { get; set; }
    public int ReputationChange { get; set; }
    public int PowerChange { get; set; }
    public DiscipleStatEffect? DiscipleStatEffects { get; set; }
    public double DiscipleCultivationBonus { get; set; }
}
