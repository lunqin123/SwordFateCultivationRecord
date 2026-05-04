namespace SwordFateCultivationRecord;

public enum AchievementCategory
{
    SectGrowth,      // 宗门发展
    DiscipleTraining,// 弟子培养
    ResourceWealth,  // 资源积累
    PlotProgress,    // 剧情推进
    Companionship,   // 道侣情缘
    Exploration,     // 秘境探索
    Challenge,       // 挑战
}

public class AchievementDef
{
    public int Id { get; set; }
    public AchievementCategory Category { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int TargetValue { get; set; }
    public int RewardReputation { get; set; }
    public bool IsHidden { get; set; } // Hidden until unlocked
}

public class AchievementProgress
{
    public List<int> UnlockedIds { get; set; } = new();
    public int TotalUnlocked => UnlockedIds.Count;
}
