namespace SwordFateCultivationRecord;

public class SectQuestSystem
{
    public const int MaxQuests = 3;
    private readonly List<SectQuestData> _quests = new();
    private int _nextId = 1;
    private readonly Random _rng = new();

    public IReadOnlyList<SectQuestData> AllQuests => _quests;

    public void Initialize()
    {
        _quests.Clear();
        for (int i = 0; i < MaxQuests; i++)
            _quests.Add(GenerateQuest(1));
    }

    /// <summary>Get the max quest tier available based on sect progress.</summary>
    public static int GetMaxTier(GameManager gm)
    {
        bool hasLibrary = gm.Facilities.AllFacilities.Any(f => f.IsBuilt && f.Type == FacilityType.Library);
        bool hasFormation = gm.Facilities.AllFacilities.Any(f => f.IsBuilt && f.Type == FacilityType.FormationHall && f.Level >= 3);
        if (gm.SectLevel >= 7 || (hasFormation && gm.SectLevel >= 5)) return 4; // 极品
        if (gm.SectLevel >= 5 || (hasLibrary && gm.SectLevel >= 3)) return 3; // 优品
        if (gm.SectLevel >= 3 || hasLibrary) return 2; // 良品
        return 1; // 凡品
    }

    /// <summary>Check quest completion. Called daily.</summary>
    public void CheckProgress(GameManager gm)
    {
        foreach (var q in _quests)
        {
            if (q.Completed) continue;
            int count = CountProgress(q.Type, gm);
            q.CurrentCount = Math.Min(count, q.TargetCount);
            if (q.CurrentCount >= q.TargetCount)
            {
                q.Completed = true;
                GrantReward(q, gm);
                EventBus.EmitNotification("宗门令达成", $"宗门任务「{q.Title}」已完成！\n奖励: {q.RewardText}");
            }
        }
    }

    /// <summary>Replace completed quests with new ones.</summary>
    public void RefreshCompleted(GameManager gm)
    {
        int maxTier = GetMaxTier(gm);
        for (int i = 0; i < _quests.Count; i++)
        {
            if (_quests[i].Completed)
                _quests[i] = GenerateQuest(_rng.Next(1, maxTier + 1));
        }
    }

    int CountProgress(QuestType type, GameManager gm)
    {
        return type switch
        {
            QuestType.RecruitDisciples => gm.Disciples.Count,
            QuestType.BuildFacilities => gm.Facilities.AllFacilities.Count(f => f.IsBuilt),
            QuestType.ReachReputation => gm.SectReputation,
            QuestType.GatherResource => gm.Resources.Get(ResourceType.SpiritStone),
            QuestType.DiscipleBreakthrough => gm.Disciples.AllDisciples.Count(d => d.Realm > CultivationRealm.Mortal),
            QuestType.UpgradeFacilities => gm.Facilities.AllFacilities.Count(f => f.Level > 1),
            _ => 0
        };
    }

    void GrantReward(SectQuestData q, GameManager gm)
    {
        gm.SectReputation += q.RewardReputation;
        foreach (var kv in q.RewardResources)
            gm.Resources.Add(kv.Key, kv.Value);
        for (int i = 0; i < q.RewardEquipment; i++)
        {
            var eq = EquipmentTable.CraftRandom(gm.SectLevel);
            gm.AllEquipment.Add(eq);
        }
    }

    SectQuestData GenerateQuest(int tier)
    {
        var types = Enum.GetValues<QuestType>();
        var type = types[_rng.Next(types.Length)];
        int target; string title, desc; var rewards = new Dictionary<ResourceType, int>(); int repReward;
        double scale = 1.0 + (tier - 1) * 0.8; // Tier multipliers: 1x, 1.8x, 2.6x, 3.4x

        switch (type)
        {
            case QuestType.RecruitDisciples:
                target = (int)((3 + _rng.Next(3)) * scale);
                title = tier switch { 1 => "广纳门徒", 2 => "招贤纳士", 3 => "英才荟萃", _ => "天下归心" };
                desc = $"招募{target}名弟子加入宗门";
                repReward = (int)(15 * target * scale);
                rewards[ResourceType.SpiritStone] = (int)(50 * target * scale);
                break;
            case QuestType.BuildFacilities:
                target = (int)((2 + _rng.Next(3)) * scale);
                title = tier switch { 1 => "宗门建设", 2 => "大兴土木", 3 => "宏图大业", _ => "万世之基" };
                desc = $"建造{target}座灵筑";
                repReward = (int)(20 * target * scale);
                rewards[ResourceType.Ore] = (int)(10 * target * scale);
                break;
            case QuestType.ReachReputation:
                target = (int)((100 + _rng.Next(5) * 50) * scale);
                title = tier switch { 1 => "声名远播", 2 => "威名赫赫", 3 => "名动一方", _ => "天下景仰" };
                desc = $"宗门声望达到{target}";
                repReward = target / 3;
                rewards[ResourceType.SpiritStone] = target;
                break;
            case QuestType.GatherResource:
                target = (int)((200 + _rng.Next(5) * 100) * scale);
                title = tier switch { 1 => "积累资源", 2 => "富甲一方", 3 => "灵石如山", _ => "富可敌国" };
                desc = $"灵石储量达到{target}";
                repReward = (int)(30 * scale);
                rewards[ResourceType.Herb] = (int)(15 * scale);
                rewards[ResourceType.Ore] = (int)(10 * scale);
                break;
            case QuestType.DiscipleBreakthrough:
                target = (int)((1 + _rng.Next(3)) * scale);
                title = tier switch { 1 => "弟子突破", 2 => "人才辈出", 3 => "精英云集", _ => "群仙毕至" };
                desc = $"培养{target}名弟子突破凡人境";
                repReward = (int)(50 * target * scale);
                rewards[ResourceType.Pill] = (int)(5 * target * scale);
                break;
            case QuestType.UpgradeFacilities:
                target = Math.Max(1, (int)((1 + _rng.Next(2)) * scale));
                title = tier switch { 1 => "灵筑晋升", 2 => "精益求精", 3 => "琼楼玉宇", _ => "仙家气象" };
                desc = $"将{target}座灵筑升至Lv.2以上";
                repReward = (int)(30 * target * scale);
                rewards[ResourceType.SpiritEssence] = (int)(20 * target * scale);
                break;
            default:
                target = 1; title = "未知"; desc = ""; repReward = 10; break;
        }

        return new SectQuestData
        {
            Id = _nextId++, Type = type, Tier = tier,
            Title = title, Description = desc, TargetCount = target,
            RewardReputation = repReward, RewardResources = rewards,
        };
    }

    public void LoadState(List<SectQuestData> quests)
    {
        _quests.Clear();
        _quests.AddRange(quests);
        _nextId = _quests.Count > 0 ? _quests.Max(q => q.Id) + 1 : 1;
    }
}
