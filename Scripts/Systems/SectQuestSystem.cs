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
            _quests.Add(GenerateQuest());
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
        for (int i = 0; i < _quests.Count; i++)
        {
            if (_quests[i].Completed)
                _quests[i] = GenerateQuest();
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

    SectQuestData GenerateQuest()
    {
        var types = Enum.GetValues<QuestType>();
        var type = types[_rng.Next(types.Length)];
        int target;
        string title, desc;
        var rewards = new Dictionary<ResourceType, int>();
        int repReward;

        switch (type)
        {
            case QuestType.RecruitDisciples:
                target = 3 + _rng.Next(3); // 3-5
                title = $"广纳门徒";
                desc = $"招募{target}名弟子加入宗门";
                repReward = 15 * target;
                rewards[ResourceType.SpiritStone] = 50 * target;
                break;
            case QuestType.BuildFacilities:
                target = 2 + _rng.Next(3); // 2-4
                title = $"宗门建设";
                desc = $"建造{target}座设施";
                repReward = 20 * target;
                rewards[ResourceType.Ore] = 10 * target;
                break;
            case QuestType.ReachReputation:
                target = 100 + _rng.Next(5) * 50; // 100-300
                title = $"声名远播";
                desc = $"宗门声望达到{target}";
                repReward = target / 3;
                rewards[ResourceType.SpiritStone] = target;
                break;
            case QuestType.GatherResource:
                target = 200 + _rng.Next(5) * 100; // 200-600
                title = $"积累资源";
                desc = $"灵石储量达到{target}";
                repReward = 30;
                rewards[ResourceType.Herb] = 15;
                rewards[ResourceType.Ore] = 10;
                break;
            case QuestType.DiscipleBreakthrough:
                target = 1 + _rng.Next(3); // 1-3
                title = $"弟子突破";
                desc = $"培养{target}名弟子突破凡人境";
                repReward = 50 * target;
                rewards[ResourceType.Pill] = 5 * target;
                break;
            case QuestType.UpgradeFacilities:
                target = 1 + _rng.Next(2); // 1-2
                title = $"灵筑晋升";
                desc = $"将{target}座设施升至Lv.2以上";
                repReward = 30 * target;
                rewards[ResourceType.SpiritEssence] = 20 * target;
                break;
            default:
                target = 1; title = "未知"; desc = ""; repReward = 10; break;
        }

        return new SectQuestData
        {
            Id = _nextId++,
            Type = type,
            Title = title,
            Description = desc,
            TargetCount = target,
            RewardReputation = repReward,
            RewardResources = rewards,
        };
    }

    public void LoadState(List<SectQuestData> quests)
    {
        _quests.Clear();
        _quests.AddRange(quests);
        _nextId = _quests.Count > 0 ? _quests.Max(q => q.Id) + 1 : 1;
    }
}
