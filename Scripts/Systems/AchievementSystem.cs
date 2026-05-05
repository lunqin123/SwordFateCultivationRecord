namespace SwordFateCultivationRecord;

public class AchievementSystem
{
    private AchievementProgress _progress = new();
    public AchievementProgress Progress => _progress;
    public int TotalCount => AchievementTable.All.Count;

    /// <summary>Check all achievements. Returns newly unlocked ones.</summary>
    public List<AchievementDef> CheckAll(GameManager gm)
    {
        var newlyUnlocked = new List<AchievementDef>();
        foreach (var ach in AchievementTable.All)
        {
            if (_progress.UnlockedIds.Contains(ach.Id)) continue;
            if (CheckCondition(ach, gm))
            {
                _progress.UnlockedIds.Add(ach.Id);
                gm.SectReputation += ach.RewardReputation;
                newlyUnlocked.Add(ach);
            }
        }
        return newlyUnlocked;
    }

    private bool CheckCondition(AchievementDef ach, GameManager gm)
    {
        return ach.Id switch
        {
            // Sect growth - level (uses TargetValue from table)
            >= 2 and <= 7 => gm.SectLevel >= ach.TargetValue,

            // Sect growth - reputation
            10 or 11 or 12 or 13 or 14 => gm.SectReputation >= ach.TargetValue,

            // Sect growth - facilities count
            15 or 16 => gm.Facilities.AllFacilities.Count(f => f.IsBuilt) >= ach.TargetValue,

            // Sect growth - facility max level
            17 or 18 => gm.Facilities.AllFacilities.Any(f => f.Level >= ach.TargetValue),

            // Disciple training - count
            20 or 21 or 22 or 23 => gm.Disciples.Count >= ach.TargetValue,

            // Disciple training - realm (TargetValue = CultivationRealm int)
            25 or 26 or 27 or 28 or 29
                => gm.Disciples.AllDisciples.Any(d => (int)d.Realm >= ach.TargetValue),

            // Disciple - outer count
            30 or 31 => gm.OuterDiscipleCount >= ach.TargetValue,

            // External trigger: outer promotion
            32 => _progress.UnlockedIds.Contains(32),

            // Resource - spirit stone
            40 or 41 or 42 => gm.Resources.Get(ResourceType.SpiritStone) >= ach.TargetValue,

            // Resource - herb / ore / pill / equipment
            43 => gm.Resources.Get(ResourceType.Herb) >= ach.TargetValue,
            44 => gm.Resources.Get(ResourceType.Ore) >= ach.TargetValue,
            45 => gm.Resources.Get(ResourceType.Pill) >= ach.TargetValue,
            46 => gm.AllEquipment.Count >= ach.TargetValue,

            // Plot progress (TargetValue = last stage id in volume)
            50 or 51 or 52 or 53 => gm.Plot.Progress.CompletedStageIds.Contains(ach.TargetValue),

            // Companion - any / married count
            60 => gm.Companions.AllCompanions.Count >= ach.TargetValue,
            61 => gm.Companions.AllCompanions.Any(c => c.IsMarried),
            62 or 63 => gm.Companions.AllCompanions.Count(c => c.IsMarried) >= ach.TargetValue,

            // Exploration - external (recorded by RecordExploration)
            70 or 71 or 72 => _progress.UnlockedIds.Contains(ach.Id),

            // Fortune child - external
            80 => _progress.UnlockedIds.Contains(80),

            // Challenge - years / power / days
            81 => gm.Time.Year >= ach.TargetValue,
            82 or 83 => gm.SectPower >= ach.TargetValue,

            // Compound challenges (time + secondary condition)
            90 => gm.Time.GetTotalDays() <= 30 && gm.SectReputation >= 100,
            91 => gm.Time.GetTotalDays() <= 100 && gm.SectLevel >= 3,

            // Total days accumulated
            92 or 93 => gm.Time.GetTotalDays() >= ach.TargetValue,

            _ => false,
        };
    }

    // External triggers
    public void TriggerFortuneChild() { if (!_progress.UnlockedIds.Contains(80)) _progress.UnlockedIds.Add(80); }
    public void TriggerOuterPromotion() { if (!_progress.UnlockedIds.Contains(32)) _progress.UnlockedIds.Add(32); }
    public void RecordExploration() { _progress.ExploreCount++; if (_progress.ExploreCount >= 1 && !_progress.UnlockedIds.Contains(70)) _progress.UnlockedIds.Add(70); if (_progress.ExploreCount >= 5 && !_progress.UnlockedIds.Contains(71)) _progress.UnlockedIds.Add(71); if (_progress.ExploreCount >= 10 && !_progress.UnlockedIds.Contains(72)) _progress.UnlockedIds.Add(72); }

    // Save/Load
    public AchievementProgress ExportProgress() => _progress;
    public void LoadProgress(AchievementProgress p) { _progress = p; }
}
