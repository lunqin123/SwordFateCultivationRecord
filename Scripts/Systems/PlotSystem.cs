namespace SwordFateCultivationRecord;

public class PlotSystem
{
    private PlotProgress _progress = new();
    private List<PlotStageDef> _stages = new();
    public PlotProgress Progress => _progress;
    public PlotStageDef? ActiveStage { get; private set; }
    public PlotStageDef? LastCompletedStage { get; private set; }

    public void Initialize()
    {
        _stages = PlotTable.AllStages;
        _progress = new PlotProgress();
        TryActivateNextStage();
    }

    public void CheckProgress(GameManager gm)
    {
        if (ActiveStage == null) return;
        if (ActiveStage.IsManualAcknowledge) return; // Wait for manual acknowledge
        if (CheckConditions(ActiveStage.CompletionConditions, gm))
            CompleteCurrentStage(gm);
    }

    public void AcknowledgeStage(GameManager gm)
    {
        if (ActiveStage == null || !ActiveStage.IsManualAcknowledge) return;
        CompleteCurrentStage(gm);
    }

    private void CompleteCurrentStage(GameManager gm)
    {
        if (ActiveStage == null) return;
        var stage = ActiveStage;

        _progress.CompletedStageIds.Add(stage.Id);
        _progress.CompletedStageCount++;

        // Grant rewards
        foreach (var kv in stage.RewardResources)
            gm.Resources.Add(kv.Key, kv.Value);
        if (stage.RewardReputation > 0)
            gm.SectReputation += stage.RewardReputation;

        LastCompletedStage = stage;
        ActiveStage = null;

        gm.LogEvent($"【剧情】{stage.Title} 完成");

        string rewardText = BuildRewardText(stage);
        string message = stage.CompletionText;
        if (!string.IsNullOrEmpty(rewardText))
            message += $"\n\n获得：{rewardText}";

        EventBus.EmitPlotStageCompleted(stage, message);

        if (stage.Id >= PlotTable.AllStages.Max(s => s.Id))
            EventBus.EmitGameEnding();

        TryActivateNextStage();
    }

    private void TryActivateNextStage()
    {
        foreach (var stage in _stages.OrderBy(s => s.Order))
        {
            if (_progress.CompletedStageIds.Contains(stage.Id)) continue;
            if (CheckTrigger(stage.TriggerCondition))
            {
                ActiveStage = stage;
                _progress.ActiveStageId = stage.Id;
                if (stage.Id != 1) // Don't auto-notify for the first stage
                    EventBus.EmitPlotStageActivated(stage);
                return;
            }
        }
    }

    private bool CheckTrigger(PlotCondition condition)
    {
        // For trigger, we check against completed stages
        if (condition.Type == PlotConditionType.None) return true;
        if (condition.Type == PlotConditionType.StageCompleted)
            return _progress.CompletedStageIds.Contains(condition.TargetValue);
        return true; // Other trigger types always pass (stage-based progression)
    }

    private bool CheckConditions(List<PlotCondition> conditions, GameManager gm)
    {
        foreach (var c in conditions)
        {
            if (!CheckSingleCondition(c, gm)) return false;
        }
        return conditions.Count > 0;
    }

    private bool CheckSingleCondition(PlotCondition c, GameManager gm)
    {
        switch (c.Type)
        {
            case PlotConditionType.None: return true;
            case PlotConditionType.StageCompleted:
                return _progress.CompletedStageIds.Contains(c.TargetValue);
            case PlotConditionType.DiscipleCount:
                return gm.Disciples.Count >= c.TargetValue;
            case PlotConditionType.FacilityCount:
                return gm.Facilities.AllFacilities.Count(f => f.IsBuilt) >= c.TargetValue;
            case PlotConditionType.DiscipleRealm:
                return gm.Disciples.AllDisciples.Any(d => (int)d.Realm >= c.TargetValue);
            case PlotConditionType.Reputation:
                return gm.SectReputation >= c.TargetValue;
            case PlotConditionType.ResourceAmount:
                return gm.Resources.Get(c.ResourceType) >= c.TargetValue;
            case PlotConditionType.SectLevel:
                return gm.SectLevel >= c.TargetValue;
            default: return false;
        }
    }

    private static string BuildRewardText(PlotStageDef stage)
    {
        var parts = new List<string>();
        if (stage.RewardReputation > 0) parts.Add($"声望+{stage.RewardReputation}");
        foreach (var kv in stage.RewardResources)
        {
            if (kv.Value > 0) parts.Add($"{ResName(kv.Key)}+{kv.Value}");
        }
        return string.Join("  ", parts);
    }

    private static string ResName(ResourceType rt) => rt switch
    {
        ResourceType.SpiritStone => "灵石", ResourceType.Herb => "灵草", ResourceType.Ore => "矿石",
        ResourceType.Pill => "丹药", ResourceType.SpiritEssence => "灵气", _ => rt.ToString()
    };

    // Save/Load
    public PlotProgress ExportProgress() => _progress;
    public void LoadProgress(PlotProgress progress)
    {
        _progress = progress;
        _stages = PlotTable.AllStages;
        ActiveStage = null;
        LastCompletedStage = null;
        if (_progress.ActiveStageId >= 0)
            ActiveStage = PlotTable.Get(_progress.ActiveStageId);
        if (_progress.CompletedStageIds.Count > 0)
        {
            int lastId = _progress.CompletedStageIds.Max();
            LastCompletedStage = PlotTable.Get(lastId);
        }
    }
}
