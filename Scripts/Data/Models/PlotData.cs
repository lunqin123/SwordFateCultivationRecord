namespace SwordFateCultivationRecord;

public enum PlotConditionType
{
    None,
    StageCompleted,
    DiscipleCount,
    FacilityCount,
    DiscipleRealm,
    Reputation,
    ResourceAmount,
    SectLevel,
}

public class PlotCondition
{
    public PlotConditionType Type { get; set; }
    public int TargetValue { get; set; }
    public ResourceType ResourceType { get; set; }
}

public class PlotStageDef
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string ChapterTitle { get; set; } = "";
    public string Narrative { get; set; } = "";
    public string Objective { get; set; } = "";
    public string CompletionHint { get; set; } = "";
    public string CompletionText { get; set; } = "";
    public PlotCondition TriggerCondition { get; set; } = new();
    public List<PlotCondition> CompletionConditions { get; set; } = new();
    public Dictionary<ResourceType, int> RewardResources { get; set; } = new();
    public int RewardReputation { get; set; }
    public bool IsManualAcknowledge =>
        CompletionConditions.Count == 1 && CompletionConditions[0].Type == PlotConditionType.None;
}

public class PlotProgress
{
    public int CurrentChapterId { get; set; } = 1;
    public int CompletedStageCount { get; set; }
    public List<int> CompletedStageIds { get; set; } = new();
    public int ActiveStageId { get; set; } = -1;
}
