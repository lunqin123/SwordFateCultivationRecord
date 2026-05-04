namespace SwordFateCultivationRecord;

public static class AchievementTable
{
    public static List<AchievementDef> All => _achievements;
    public static AchievementDef? Get(int id) => _achievements.FirstOrDefault(a => a.Id == id);

    private static readonly List<AchievementDef> _achievements = new()
    {
        // ===== 宗门发展 =====
        new() { Id = 1, Category = AchievementCategory.SectGrowth, Title = "开宗立派", Description = "创立宗门", TargetValue = 1, RewardReputation = 10 },
        new() { Id = 2, Category = AchievementCategory.SectGrowth, Title = "初露锋芒", Description = "宗门达到Lv.2", TargetValue = 2, RewardReputation = 30 },
        new() { Id = 3, Category = AchievementCategory.SectGrowth, Title = "小有名气", Description = "宗门达到Lv.3", TargetValue = 3, RewardReputation = 50 },
        new() { Id = 4, Category = AchievementCategory.SectGrowth, Title = "一方豪强", Description = "宗门达到Lv.4", TargetValue = 4, RewardReputation = 80 },
        new() { Id = 5, Category = AchievementCategory.SectGrowth, Title = "名门大派", Description = "宗门达到Lv.5", TargetValue = 5, RewardReputation = 120 },
        new() { Id = 6, Category = AchievementCategory.SectGrowth, Title = "威震四方", Description = "宗门达到Lv.6", TargetValue = 6, RewardReputation = 200 },
        new() { Id = 7, Category = AchievementCategory.SectGrowth, Title = "仙道圣地", Description = "宗门达到Lv.7", TargetValue = 7, RewardReputation = 300 },

        new() { Id = 10, Category = AchievementCategory.SectGrowth, Title = "初具规模", Description = "声望达到100", TargetValue = 100, RewardReputation = 10 },
        new() { Id = 11, Category = AchievementCategory.SectGrowth, Title = "声名远播", Description = "声望达到500", TargetValue = 500, RewardReputation = 30 },
        new() { Id = 12, Category = AchievementCategory.SectGrowth, Title = "名动一方", Description = "声望达到1000", TargetValue = 1000, RewardReputation = 50 },
        new() { Id = 13, Category = AchievementCategory.SectGrowth, Title = "天下景仰", Description = "声望达到3000", TargetValue = 3000, RewardReputation = 100 },
        new() { Id = 14, Category = AchievementCategory.SectGrowth, Title = "万宗至尊", Description = "声望达到6000", TargetValue = 6000, RewardReputation = 200 },

        new() { Id = 15, Category = AchievementCategory.SectGrowth, Title = "欣欣向荣", Description = "建造5座灵筑", TargetValue = 5, RewardReputation = 20 },
        new() { Id = 16, Category = AchievementCategory.SectGrowth, Title = "灵筑林立", Description = "建造10座灵筑", TargetValue = 10, RewardReputation = 50 },
        new() { Id = 17, Category = AchievementCategory.SectGrowth, Title = "登峰造极", Description = "任意灵筑升至Lv.5", TargetValue = 5, RewardReputation = 40 },
        new() { Id = 18, Category = AchievementCategory.SectGrowth, Title = "极致之道", Description = "任意灵筑升至Lv.10", TargetValue = 10, RewardReputation = 100 },

        // ===== 弟子培养 =====
        new() { Id = 20, Category = AchievementCategory.DiscipleTraining, Title = "初收门徒", Description = "内门弟子达到3人", TargetValue = 3, RewardReputation = 15 },
        new() { Id = 21, Category = AchievementCategory.DiscipleTraining, Title = "人才济济", Description = "内门弟子达到8人", TargetValue = 8, RewardReputation = 30 },
        new() { Id = 22, Category = AchievementCategory.DiscipleTraining, Title = "英才荟萃", Description = "内门弟子达到15人", TargetValue = 15, RewardReputation = 60 },
        new() { Id = 23, Category = AchievementCategory.DiscipleTraining, Title = "天下归心", Description = "内门弟子达到25人", TargetValue = 25, RewardReputation = 100 },

        new() { Id = 25, Category = AchievementCategory.DiscipleTraining, Title = "初窥门径", Description = "拥有练气期弟子", TargetValue = (int)CultivationRealm.QiRefining, RewardReputation = 20 },
        new() { Id = 26, Category = AchievementCategory.DiscipleTraining, Title = "根基稳固", Description = "拥有筑基期弟子", TargetValue = (int)CultivationRealm.Foundation, RewardReputation = 50 },
        new() { Id = 27, Category = AchievementCategory.DiscipleTraining, Title = "金丹大道", Description = "拥有金丹期弟子", TargetValue = (int)CultivationRealm.CoreFormation, RewardReputation = 100 },
        new() { Id = 28, Category = AchievementCategory.DiscipleTraining, Title = "元婴出世", Description = "拥有元婴期弟子", TargetValue = (int)CultivationRealm.NascentSoul, RewardReputation = 200 },
        new() { Id = 29, Category = AchievementCategory.DiscipleTraining, Title = "化神之境", Description = "拥有化神期弟子", TargetValue = (int)CultivationRealm.SpiritTransformation, RewardReputation = 400 },

        new() { Id = 30, Category = AchievementCategory.DiscipleTraining, Title = "外门兴旺", Description = "外门弟子达到50人", TargetValue = 50, RewardReputation = 20 },
        new() { Id = 31, Category = AchievementCategory.DiscipleTraining, Title = "万众归心", Description = "外门弟子达到200人", TargetValue = 200, RewardReputation = 60 },
        new() { Id = 32, Category = AchievementCategory.DiscipleTraining, Title = "外门英才", Description = "外门弟子晋升内门", TargetValue = 1, RewardReputation = 15 },

        // ===== 资源积累 =====
        new() { Id = 40, Category = AchievementCategory.ResourceWealth, Title = "小有积蓄", Description = "灵石达到1000", TargetValue = 1000, RewardReputation = 10 },
        new() { Id = 41, Category = AchievementCategory.ResourceWealth, Title = "富甲一方", Description = "灵石达到5000", TargetValue = 5000, RewardReputation = 30 },
        new() { Id = 42, Category = AchievementCategory.ResourceWealth, Title = "富可敌国", Description = "灵石达到20000", TargetValue = 20000, RewardReputation = 80 },

        new() { Id = 43, Category = AchievementCategory.ResourceWealth, Title = "灵草满仓", Description = "灵草达到100", TargetValue = 100, RewardReputation = 15 },
        new() { Id = 44, Category = AchievementCategory.ResourceWealth, Title = "矿石如山", Description = "矿石达到100", TargetValue = 100, RewardReputation = 15 },
        new() { Id = 45, Category = AchievementCategory.ResourceWealth, Title = "丹道有成", Description = "丹药达到50", TargetValue = 50, RewardReputation = 25 },
        new() { Id = 46, Category = AchievementCategory.ResourceWealth, Title = "法器满堂", Description = "法器达到30件", TargetValue = 30, RewardReputation = 30 },

        // ===== 剧情推进 =====
        new() { Id = 50, Category = AchievementCategory.PlotProgress, Title = "凡尘初醒", Description = "完成卷一全部剧情", TargetValue = 5, RewardReputation = 30 },
        new() { Id = 51, Category = AchievementCategory.PlotProgress, Title = "崭露头角", Description = "完成卷二全部剧情", TargetValue = 10, RewardReputation = 60 },
        new() { Id = 52, Category = AchievementCategory.PlotProgress, Title = "威震一方", Description = "完成卷三全部剧情", TargetValue = 15, RewardReputation = 100 },
        new() { Id = 53, Category = AchievementCategory.PlotProgress, Title = "问道长生", Description = "完成全部四卷剧情", TargetValue = 20, RewardReputation = 200 },

        // ===== 道侣情缘 =====
        new() { Id = 60, Category = AchievementCategory.Companionship, Title = "初次牵线", Description = "促成一桩结缘", TargetValue = 1, RewardReputation = 10 },
        new() { Id = 61, Category = AchievementCategory.Companionship, Title = "天赐良缘", Description = "道侣成功结婚", TargetValue = 1, RewardReputation = 20 },
        new() { Id = 62, Category = AchievementCategory.Companionship, Title = "情比金坚", Description = "拥有3对道侣", TargetValue = 3, RewardReputation = 50 },
        new() { Id = 63, Category = AchievementCategory.Companionship, Title = "宗门世家", Description = "拥有5对道侣", TargetValue = 5, RewardReputation = 100 },

        // ===== 秘境探索 =====
        new() { Id = 70, Category = AchievementCategory.Exploration, Title = "初探秘境", Description = "完成1次秘境探索", TargetValue = 1, RewardReputation = 15 },
        new() { Id = 71, Category = AchievementCategory.Exploration, Title = "秘境常客", Description = "完成5次秘境探索", TargetValue = 5, RewardReputation = 40 },
        new() { Id = 72, Category = AchievementCategory.Exploration, Title = "秘境行者", Description = "完成10次秘境探索", TargetValue = 10, RewardReputation = 80 },

        // ===== 挑战 =====
        new() { Id = 80, Category = AchievementCategory.Challenge, Title = "气运加身", Description = "在内门选拔中发现气运之子", TargetValue = 1, RewardReputation = 50, IsHidden = true },
        new() { Id = 81, Category = AchievementCategory.Challenge, Title = "百年宗门", Description = "游戏时间达到100年", TargetValue = 100, RewardReputation = 100 },
        new() { Id = 82, Category = AchievementCategory.Challenge, Title = "战力超群", Description = "宗门战力达到1000", TargetValue = 1000, RewardReputation = 75 },
        new() { Id = 83, Category = AchievementCategory.Challenge, Title = "所向披靡", Description = "宗门战力达到5000", TargetValue = 5000, RewardReputation = 200 },

        new() { Id = 90, Category = AchievementCategory.Challenge, Title = "完美开局", Description = "开局30天内声望达到100", TargetValue = 30, RewardReputation = 25 },
        new() { Id = 91, Category = AchievementCategory.Challenge, Title = "快速崛起", Description = "100天内宗门达到Lv.3", TargetValue = 100, RewardReputation = 50 },
        new() { Id = 92, Category = AchievementCategory.Challenge, Title = "日积月累", Description = "累计推演1000天", TargetValue = 1000, RewardReputation = 50 },
        new() { Id = 93, Category = AchievementCategory.Challenge, Title = "时光荏苒", Description = "累计推演5000天", TargetValue = 5000, RewardReputation = 150 },
    };
}
