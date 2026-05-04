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
        switch (ach.Id)
        {
            // Sect growth - level
            case 2: return gm.SectLevel >= 2;
            case 3: return gm.SectLevel >= 3;
            case 4: return gm.SectLevel >= 4;
            case 5: return gm.SectLevel >= 5;
            case 6: return gm.SectLevel >= 6;
            case 7: return gm.SectLevel >= 7;

            // Sect growth - reputation
            case 10: return gm.SectReputation >= 100;
            case 11: return gm.SectReputation >= 500;
            case 12: return gm.SectReputation >= 1000;
            case 13: return gm.SectReputation >= 3000;
            case 14: return gm.SectReputation >= 6000;

            // Sect growth - facilities
            case 15: return gm.Facilities.AllFacilities.Count(f => f.IsBuilt) >= 5;
            case 16: return gm.Facilities.AllFacilities.Count(f => f.IsBuilt) >= 10;
            case 17: return gm.Facilities.AllFacilities.Any(f => f.Level >= 5);
            case 18: return gm.Facilities.AllFacilities.Any(f => f.Level >= 10);

            // Disciple training - count
            case 20: return gm.Disciples.Count >= 3;
            case 21: return gm.Disciples.Count >= 8;
            case 22: return gm.Disciples.Count >= 15;
            case 23: return gm.Disciples.Count >= 25;

            // Disciple training - realm
            case 25: return gm.Disciples.AllDisciples.Any(d => (int)d.Realm >= (int)CultivationRealm.QiRefining);
            case 26: return gm.Disciples.AllDisciples.Any(d => (int)d.Realm >= (int)CultivationRealm.Foundation);
            case 27: return gm.Disciples.AllDisciples.Any(d => (int)d.Realm >= (int)CultivationRealm.CoreFormation);
            case 28: return gm.Disciples.AllDisciples.Any(d => (int)d.Realm >= (int)CultivationRealm.NascentSoul);
            case 29: return gm.Disciples.AllDisciples.Any(d => (int)d.Realm >= (int)CultivationRealm.SpiritTransformation);

            // Disciple - outer
            case 30: return gm.OuterDiscipleCount >= 50;
            case 31: return gm.OuterDiscipleCount >= 200;
            case 32: return _progress.UnlockedIds.Contains(32); // Set externally when promotion happens

            // Resource
            case 40: return gm.Resources.Get(ResourceType.SpiritStone) >= 1000;
            case 41: return gm.Resources.Get(ResourceType.SpiritStone) >= 5000;
            case 42: return gm.Resources.Get(ResourceType.SpiritStone) >= 20000;
            case 43: return gm.Resources.Get(ResourceType.Herb) >= 100;
            case 44: return gm.Resources.Get(ResourceType.Ore) >= 100;
            case 45: return gm.Resources.Get(ResourceType.Pill) >= 50;
            case 46: return gm.AllEquipment.Count >= 30;

            // Plot
            case 50: return gm.Plot.Progress.CompletedStageIds.Contains(5);
            case 51: return gm.Plot.Progress.CompletedStageIds.Contains(10);
            case 52: return gm.Plot.Progress.CompletedStageIds.Contains(15);
            case 53: return gm.Plot.Progress.CompletedStageIds.Contains(20);

            // Companion
            case 60: return gm.Companions.AllCompanions.Count >= 1;
            case 61: return gm.Companions.AllCompanions.Any(c => c.IsMarried);
            case 62: return gm.Companions.AllCompanions.Count(c => c.IsMarried) >= 3;
            case 63: return gm.Companions.AllCompanions.Count(c => c.IsMarried) >= 5;

            // Exploration - tracked via a counter
            case 70: case 71: case 72:
                return _progress.UnlockedIds.Contains(ach.Id); // Set externally

            // Challenge
            case 80: return _progress.UnlockedIds.Contains(80); // Fortune child - set externally
            case 81: return gm.Time.Year >= 100;
            case 82: return gm.SectPower >= 1000;
            case 83: return gm.SectPower >= 5000;
            case 90: return gm.Time.GetTotalDays() <= 30 && gm.SectReputation >= 100;
            case 91: return gm.Time.GetTotalDays() <= 100 && gm.SectLevel >= 3;
            case 92: return gm.Time.GetTotalDays() >= 1000;
            case 93: return gm.Time.GetTotalDays() >= 5000;

            default: return false;
        }
    }

    // External triggers
    public void TriggerFortuneChild() { if (!_progress.UnlockedIds.Contains(80)) _progress.UnlockedIds.Add(80); }
    public void TriggerOuterPromotion() { if (!_progress.UnlockedIds.Contains(32)) _progress.UnlockedIds.Add(32); }
    private int _exploreCount;
    public void RecordExploration() { _exploreCount++; if (_exploreCount >= 1 && !_progress.UnlockedIds.Contains(70)) _progress.UnlockedIds.Add(70); if (_exploreCount >= 5 && !_progress.UnlockedIds.Contains(71)) _progress.UnlockedIds.Add(71); if (_exploreCount >= 10 && !_progress.UnlockedIds.Contains(72)) _progress.UnlockedIds.Add(72); }

    // Save/Load
    public AchievementProgress ExportProgress() => _progress;
    public void LoadProgress(AchievementProgress p) { _progress = p; }
}
