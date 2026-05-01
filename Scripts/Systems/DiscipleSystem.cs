namespace SwordFateCultivationRecord;

public class DiscipleSystem
{
    private readonly List<DiscipleData> _disciples = new();
    private int _nextId = 1;
    private readonly Random _rng = new();

    public IReadOnlyList<DiscipleData> AllDisciples => _disciples;
    public int Count => _disciples.Count;

    public void AddChildDisciple(DiscipleData child)
    {
        child.Id = _nextId++;
        _disciples.Add(child);
        EventBus.EmitDiscipleRecruited(child);
    }

    public DiscipleData Recruit(string? name = null)
    {
        var (talent, comprehension, constitution, spirit) = DiscipleNameTable.GenerateStats();
        bool isMale = _rng.Next(2) == 0;
        var disciple = new DiscipleData
        {
            Id = _nextId++,
            Name = name ?? DiscipleNameTable.GenerateName(isMale),
            Age = _rng.Next(14, 25),
            Talent = talent,
            Comprehension = comprehension,
            Constitution = constitution,
            Spirit = spirit,
            Realm = CultivationRealm.Mortal,
            IsMale = isMale,
            SpiritRoot = RollSpiritRoot(false),
        };
        _disciples.Add(disciple);
        EventBus.EmitDiscipleRecruited(disciple);
        return disciple;
    }

    public bool Dismiss(int discipleId)
    {
        var d = Get(discipleId);
        if (d == null) return false;
        // Clear companion link if exists
        if (d.CompanionId >= 0)
        {
            // Find companion and clear their link too
            foreach (var other in _disciples)
            {
                if (other.CompanionId == d.CompanionId)
                {
                    other.CompanionId = -1;
                    other.IsMarried = false;
                    other.Mood = Math.Max(0, other.Mood - 20);
                }
            }
            d.CompanionId = -1;
            d.IsMarried = false;
        }
        _disciples.Remove(d);
        EventBus.EmitDiscipleDeparted(d);
        return true;
    }

    public DiscipleData? Get(int id) => _disciples.FirstOrDefault(d => d.Id == id);

    public void AssignTask(int discipleId, DiscipleTaskType task, int targetId = -1)
    {
        var d = Get(discipleId);
        if (d == null) return;
        d.CurrentTask = task;
        d.TaskTargetId = targetId;
        d.TaskProgress = 0;
    }

    // Skill IDs: 1=修炼加速 2=战斗技巧 3=炼丹精通 4=炼器精通 5=采集专精 6=教导有方 7=探索老手
    private static readonly int[] SkillIdCultivate = { 1 };
    private static readonly int[] SkillIdTrain = { 2 };
    private static readonly int[] SkillIdAlchemy = { 3 };
    private static readonly int[] SkillIdCraft = { 4 };
    private static readonly int[] SkillIdGather = { 5 };
    private static readonly int[] SkillIdTeach = { 6 };
    private static readonly int[] SkillIdExplore = { 7 };

    public List<EquipmentData> NewEquipmentToday { get; } = new();

    /// <summary>Process a day for all disciples. Returns sect power gained from Guard tasks.</summary>
    public int ProcessDaily(ResourceSystem resources, IEnumerable<FacilityData>? facilities = null, int sectLevel = 1,
        Dictionary<int, double>? companionCultivationBonus = null)
    {
        NewEquipmentToday.Clear();
        // Pre-compute task bonuses from built facilities
        var taskBonus = new double[9];
        if (facilities != null)
        {
            for (int i = 0; i < 9; i++)
                taskBonus[i] = FacilityTable.GetTotalTaskBonus((DiscipleTaskType)i, facilities);
        }

        int powerGain = 0;

        foreach (var d in _disciples)
        {
            if (d.IsInBreakthrough)
            {
                ProcessBreakthrough(d, resources);
                continue;
            }

            // Rest: recover stamina, skip other processing
            if (d.CurrentTask == DiscipleTaskType.Rest)
            {
                d.CurrentStamina = Math.Min(d.MaxStamina, d.CurrentStamina + 20);
                d.Mood = Math.Min(100, d.Mood + 2);
                continue;
            }

            // Increment task proficiency (chance based on task progress)
            if (d.CurrentTask != DiscipleTaskType.Rest)
            {
                d.TaskProficiency.TryGetValue(d.CurrentTask, out int prof);
                if (prof < 5 && _rng.NextDouble() < 0.02) // 2% per day
                    d.TaskProficiency[d.CurrentTask] = prof + 1;
            }

            // All non-rest tasks consume stamina
            d.CurrentStamina -= 10;
            if (d.CurrentStamina <= 0)
            {
                d.CurrentStamina = 0;
                d.Mood = Math.Max(0, d.Mood - 5);
                continue;
            }

            // Mood naturally drifts toward 50
            d.Mood += (50 - d.Mood) * 0.02;
            d.Mood = Math.Clamp(d.Mood, 0, 100);

            double bonus = 1.0 + taskBonus[(int)d.CurrentTask];
            d.TaskProficiency.TryGetValue(d.CurrentTask, out int proficiency);
            double profBonus = 1.0 + proficiency * 0.08; // +8% per proficiency level
            bonus *= profBonus;

            switch (d.CurrentTask)
            {
                case DiscipleTaskType.Cultivate:
                    d.TotalContribution++;
                    double compBonus = companionCultivationBonus?.GetValueOrDefault(d.Id, 0) ?? 0;
                    ProcessCultivate(d, bonus, compBonus);
                    TryLearnSkill(d, SkillIdCultivate);
                    break;
                case DiscipleTaskType.Train:
                    d.TotalContribution += 2;
                    powerGain += ProcessTrain(d, bonus);
                    TryLearnSkill(d, SkillIdTrain);
                    break;
                case DiscipleTaskType.Gather:
                    d.TotalContribution += 2;
                    ProcessGather(d, resources, bonus);
                    TryLearnSkill(d, SkillIdGather);
                    break;
                case DiscipleTaskType.Alchemy:
                    d.TotalContribution += 3;
                    ProcessAlchemy(d, resources, bonus);
                    TryLearnSkill(d, SkillIdAlchemy);
                    break;
                case DiscipleTaskType.Craft:
                    d.TotalContribution += 3;
                    ProcessCraft(d, resources, bonus, sectLevel);
                    TryLearnSkill(d, SkillIdCraft);
                    break;
                case DiscipleTaskType.Teach:
                    d.TotalContribution += 2;
                    ProcessTeach(d, bonus);
                    TryLearnSkill(d, SkillIdTeach);
                    break;
                case DiscipleTaskType.Guard:
                    d.TotalContribution += 2;
                    powerGain += ProcessGuard(d, bonus);
                    break;
                case DiscipleTaskType.Explore:
                    d.TotalContribution += 3;
                    ProcessExplore(d, resources, bonus);
                    TryLearnSkill(d, SkillIdExplore);
                    break;
            }
        }

        // Check for departures due to low mood/loyalty
        CheckDepartures();

        return powerGain;
    }

    void CheckDepartures()
    {
        var toRemove = new List<DiscipleData>();
        foreach (var d in _disciples)
        {
            // Track consecutive low mood days
            if (d.Mood < 15)
                d.ConsecutiveLowMoodDays++;
            else
                d.ConsecutiveLowMoodDays = 0;

            // Departure risk
            double risk = 0;
            if (d.ConsecutiveLowMoodDays >= 3) risk += 0.08; // 8% per day after 3 days of low mood
            if (d.Loyalty < 15) risk += 0.05;
            if (d.ConsecutiveLowMoodDays >= 3 && d.Loyalty < 15) risk += 0.12;
            if (d.ConsecutiveLowMoodDays >= 7) risk = 0.3; // 30% after a week of misery

            if (risk > 0 && _rng.NextDouble() < risk)
            {
                toRemove.Add(d);
                string reason = d.ConsecutiveLowMoodDays >= 3 && d.Loyalty < 15
                    ? $"{d.Name}心情长期低落且对宗门已无忠心，留下一封书信悄然离去。"
                    : d.ConsecutiveLowMoodDays >= 3
                    ? $"{d.Name}因心情持续低落，决定离开宗门另寻出路。"
                    : $"{d.Name}对宗门失去了忠诚，不告而别。";
                EventBus.EmitNotification("弟子出走", reason);
            }
        }
        foreach (var d in toRemove)
        {
            // Clear companion links
            if (d.CompanionId >= 0)
            {
                foreach (var other in _disciples)
                {
                    if (other.CompanionId == d.CompanionId)
                    {
                        other.CompanionId = -1;
                        other.IsMarried = false;
                        other.Mood = Math.Max(0, other.Mood - 25);
                    }
                }
            }
            _disciples.Remove(d);
            EventBus.EmitDiscipleDeparted(d);
        }
    }

    private void TryLearnSkill(DiscipleData d, int[] skillIds)
    {
        foreach (int skillId in skillIds)
        {
            if (d.Skills.ContainsKey(skillId)) continue;
            // Base 2% chance per day, influenced by Comprehension
            double chance = 0.02 * (1 + d.Comprehension / 200.0);
            if (_rng.NextDouble() < chance)
                d.Skills[skillId] = 1;
        }
    }

    // ===== Task Processing =====

    private double SkillBonus(DiscipleData d, int skillId, double perLevel)
    {
        d.Skills.TryGetValue(skillId, out int level);
        return 1.0 + level * perLevel;
    }

    private void ProcessCultivate(DiscipleData d, double bonus = 1.0, double companionBonus = 0)
    {
        var info = CultivationTable.GetInfo(d.Realm);
        double speed = info.BaseCultivationSpeed * bonus * (1.0 + companionBonus);
        speed *= 1.0 + d.EffTalent / 200.0;
        speed *= 1.0 + d.Mood / 200.0;
        speed *= 1.0 + d.EffComprehension / 400.0;
        speed *= 1.0 + d.EffCultivationSpeedBonus; // equipment cultivation bonus
        speed *= 1.0 + d.SpiritRootCultivationBonus; // spirit root bonus
        speed *= SkillBonus(d, 1, 0.10); // 修炼加速 +10% per level
        speed *= SkillBonus(d, 10, 0.10); // 道脉传承 +10% per level
        d.CultivationProgress += speed;

        if (d.CultivationProgress >= info.BreakthroughRequiredProgress
            && !CultivationTable.IsMaxRealm(d.Realm, d.RealmLayer))
        {
            d.IsInBreakthrough = true;
            d.BreakthroughProgress = 0;
        }
    }

    /// <summary>Train: gain combat exp, occasionally boost Constitution.</summary>
    private int ProcessTrain(DiscipleData d, double bonus = 1.0)
    {
        d.TaskProgress += (d.EffConstitution * 0.5 + _rng.NextDouble() * 5) * bonus;
        if (d.TaskProgress >= 100)
        {
            d.TaskProgress -= 100;
            d.Constitution = Math.Min(100, d.Constitution + 1);
        }
        double sk = SkillBonus(d, 2, 0.05);
        return (int)(d.CombatPower * 0.01 * sk) + 1;
    }

    /// <summary>Gather: produce herbs or ore based on Spirit.</summary>
    private void ProcessGather(DiscipleData d, ResourceSystem resources, double bonus = 1.0)
    {
        double sk = SkillBonus(d, 5, 0.15);
        int amount = (int)((1 + d.EffSpirit / 20.0 + _rng.Next(0, 3)) * bonus * sk);
        // 40% herb, 30% ore, 30% both
        int roll = _rng.Next(100);
        if (roll < 40)
            resources.Add(ResourceType.Herb, amount);
        else if (roll < 70)
            resources.Add(ResourceType.Ore, amount);
        else
        {
            resources.Add(ResourceType.Herb, Math.Max(1, amount / 2));
            resources.Add(ResourceType.Ore, Math.Max(1, amount / 2));
        }
        d.Mood = Math.Min(100, d.Mood + 1);
    }

    /// <summary>Alchemy: convert 2 herbs → 1 pill (extra yield from high Spirit).</summary>
    private void ProcessAlchemy(DiscipleData d, ResourceSystem resources, double facilityBonus = 1.0)
    {
        int herbs = resources.Get(ResourceType.Herb);
        if (herbs < 2) { d.Mood = Math.Max(0, d.Mood - 3); return; }

        int consumed = Math.Min(herbs, Math.Max(2, (int)(4 * facilityBonus))); // more herbs processed with bonus
        resources.Spend(ResourceType.Herb, consumed);

        double skAl = SkillBonus(d, 3, 0.15);
        int pills = (int)(consumed / 2 * facilityBonus * skAl);
        double spiritBonus = d.EffSpirit / 200.0;
        if (_rng.NextDouble() < spiritBonus) pills++;

        resources.Add(ResourceType.Pill, pills);
        d.TaskProgress += pills;
        if (d.TaskProgress >= 20)
        {
            d.TaskProgress -= 20;
            d.Spirit = Math.Min(100, d.Spirit + 1);
        }
    }

    /// <summary>Craft: consume ore to create equipment items.</summary>
    private void ProcessCraft(DiscipleData d, ResourceSystem resources, double bonus = 1.0, int sectLevel = 1)
    {
        int ore = resources.Get(ResourceType.Ore);
        if (ore < 2) { d.Mood = Math.Max(0, d.Mood - 3); return; }

        int consumed = Math.Min(ore, Math.Max(2, (int)(4 * bonus)));
        resources.Spend(ResourceType.Ore, consumed);

        double skCr = SkillBonus(d, 4, 0.15);
        int crafts = (int)(consumed / 2 * bonus * skCr);
        double statBonus = d.EffConstitution / 200.0;
        if (_rng.NextDouble() < statBonus) crafts++;

        for (int i = 0; i < crafts; i++)
        {
            var eq = EquipmentTable.CraftRandom(sectLevel);
            eq.Name = $"{d.Name}炼·{eq.Name}";
            NewEquipmentToday.Add(eq);
        }

        d.TaskProgress += crafts;
        if (d.TaskProgress >= 15)
        {
            d.TaskProgress -= 15;
            d.Talent = Math.Min(100, d.Talent + 1);
        }
    }

    /// <summary>Teach: boost cultivation of all OTHER disciples.</summary>
    private void ProcessTeach(DiscipleData d, double bonus = 1.0)
    {
        double skTeach = SkillBonus(d, 6, 0.10);
        double boost = (2 + d.EffComprehension * 0.1 + d.EffTalent * 0.05) * bonus * skTeach;
        foreach (var other in _disciples)
        {
            if (other.Id == d.Id) continue;
            other.CultivationProgress += boost;
        }
        d.Mood = Math.Min(100, d.Mood + 3);
    }

    /// <summary>Guard: protect the sect, gain power and small spirit stones.</summary>
    private int ProcessGuard(DiscipleData d, double bonus = 1.0)
    {
        int power = (int)((1 + d.CombatPower / 200.0) * bonus);
        d.TaskProgress += power;
        if (d.TaskProgress >= 30)
        {
            d.TaskProgress -= 30;
            d.Loyalty = Math.Min(100, d.Loyalty + 2);
        }
        return power;
    }

    /// <summary>Explore: find resources with risk of injury.</summary>
    private void ProcessExplore(DiscipleData d, ResourceSystem resources, double bonus = 1.0)
    {
        // Risk of injury reduced by facility bonus and exploration skill
        double skEx = SkillBonus(d, 7, -0.10); // -10% injury risk per level
        if (_rng.NextDouble() < 0.20 * (1 - d.EffConstitution / 200.0) / bonus * skEx)
        {
            d.Health = Math.Max(1, d.Health - _rng.Next(5, 20));
            d.Mood = Math.Max(0, d.Mood - 5);
            resources.Add(ResourceType.SpiritStone, 5);
            return;
        }

        double skLoot = SkillBonus(d, 7, 0.12);
        int loot = (int)((_rng.Next(1, 5) + d.EffSpirit / 25.0) * bonus * skLoot);
        int roll = _rng.Next(100);
        if (roll < 30)
            resources.Add(ResourceType.SpiritStone, loot * 10);
        else if (roll < 55)
            resources.Add(ResourceType.Herb, loot);
        else if (roll < 75)
            resources.Add(ResourceType.Ore, loot);
        else if (roll < 90)
            resources.Add(ResourceType.Pill, Math.Max(1, loot / 2));
        else
        {
            // Jackpot: find rare resources or cultivation boost
            resources.Add(ResourceType.SpiritStone, loot * 20);
            d.CultivationProgress += 10;
        }

        d.TaskProgress += loot;
        if (d.TaskProgress >= 25)
        {
            d.TaskProgress -= 25;
            d.Comprehension = Math.Min(100, d.Comprehension + 1);
        }
    }

    // ===== Breakthrough =====

    private void ProcessBreakthrough(DiscipleData d, ResourceSystem resources)
    {
        var info = CultivationTable.GetInfo(d.Realm);

        if (!resources.Spend(ResourceType.SpiritStone, info.BreakthroughCost))
        {
            d.IsInBreakthrough = false;
            d.Mood = Math.Max(0, d.Mood - 10);
            return;
        }

        double chance = info.BreakthroughChance;
        chance *= 1.0 + (d.Comprehension - 50) / 200.0;
        chance = Math.Clamp(chance, 0.05, 0.95);

        if (_rng.NextDouble() < chance)
        {
            d.RealmLayer++;
            if (d.RealmLayer > CultivationTable.MaxRealmLayer(d.Realm))
            {
                d.Realm = d.Realm + 1;
                d.RealmLayer = 1;
            }
            d.CultivationProgress = 0;
            d.IsInBreakthrough = false;
            d.MaxStamina = CultivationTable.GetInfo(d.Realm).MaxStamina;
            d.CurrentStamina = Math.Min(d.MaxStamina, d.CurrentStamina + 30);
            EventBus.EmitBreakthroughSuccess(d);
        }
        else
        {
            d.BreakthroughProgress += info.BreakthroughRequiredProgress * 0.1;
            d.Mood = Math.Max(0, d.Mood - 15);
            d.Health = Math.Max(1, d.Health - 10);
            if (_rng.NextDouble() < 0.2)
            {
                if (d.RealmLayer > 1) d.RealmLayer--;
                else if (d.Realm > CultivationRealm.Mortal)
                {
                    d.Realm--;
                    d.RealmLayer = CultivationTable.MaxRealmLayer(d.Realm);
                }
            }
            EventBus.EmitBreakthroughFailed(d);
        }
    }

    public SpiritualRoot RollSpiritRoot(bool isChild)
    {
        double roll = _rng.NextDouble();
        if (isChild)
        {
            // Children have better odds (one tier higher minimum)
            if (roll < 0.02) return SpiritualRoot.Heavenly;
            if (roll < 0.08) return SpiritualRoot.SingleElement;
            if (roll < 0.20) return SpiritualRoot.DualElement;
            if (roll < 0.50) return SpiritualRoot.ThreeElement;
            if (roll < 0.52) return SpiritualRoot.Special;
            return SpiritualRoot.None;
        }
        else
        {
            if (roll < 0.005) return SpiritualRoot.Heavenly;
            if (roll < 0.02) return SpiritualRoot.SingleElement;
            if (roll < 0.06) return SpiritualRoot.DualElement;
            if (roll < 0.18) return SpiritualRoot.ThreeElement;
            if (roll < 0.185) return SpiritualRoot.Special;
            return SpiritualRoot.None;
        }
    }

    public void LoadState(List<DiscipleData> disciples)
    {
        _disciples.Clear();
        _disciples.AddRange(disciples);
        _nextId = _disciples.Count > 0 ? _disciples.Max(d => d.Id) + 1 : 1;
    }
}
