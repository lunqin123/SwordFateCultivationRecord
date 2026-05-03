namespace SwordFateCultivationRecord;

public class CompanionSystem
{
    private readonly List<CompanionData> _companions = new();
    private int _nextId = 1;
    private readonly Random _rng = new();

    public IReadOnlyList<CompanionData> AllCompanions => _companions;

    /// <summary>Try to introduce two disciples as companions. Returns the new CompanionData or null.</summary>
    public CompanionData? Introduce(int disciple1Id, int disciple2Id, List<DiscipleData> allDisciples)
    {
        var d1 = allDisciples.FirstOrDefault(d => d.Id == disciple1Id);
        var d2 = allDisciples.FirstOrDefault(d => d.Id == disciple2Id);
        if (d1 == null || d2 == null) return null;
        if (d1.CompanionId >= 0 || d2.CompanionId >= 0) return null;
        if (d1.Id == d2.Id) return null;
        if (d1.IsMale == d2.IsMale) return null; // only opposite gender for traditional cultivation setting

        // Compatibility score determines success
        double compat = CalculateCompatibility(d1, d2);
        if (_rng.NextDouble() > compat)
        {
            EventBus.EmitNotification("牵线未果", $"{d1.Name}与{d2.Name}似乎不太投缘，未能结成道缘。");
            return null;
        }

        var companion = new CompanionData
        {
            Id = _nextId++,
            DiscipleId1 = disciple1Id,
            DiscipleId2 = disciple2Id,
            Affection = 25 + compat * 20, // start with 25-45 affection based on compatibility
        };
        d1.CompanionId = companion.Id;
        d2.CompanionId = companion.Id;
        _companions.Add(companion);

        EventBus.EmitNotification("道缘初结", $"{d1.Name}与{d2.Name}一见如故，结下了道缘！");
        return companion;
    }

    /// <summary>Propose marriage (结为道侣) - requires affection >= 60.</summary>
    public bool ProposeMarriage(int companionId, List<DiscipleData> allDisciples)
    {
        var c = Get(companionId);
        if (c == null || c.IsMarried) return false;
        if (c.Affection < 60) return false;

        c.IsMarried = true;
        var d1 = allDisciples.FirstOrDefault(d => d.Id == c.DiscipleId1);
        var d2 = allDisciples.FirstOrDefault(d => d.Id == c.DiscipleId2);
        if (d1 != null) d1.IsMarried = true;
        if (d2 != null) d2.IsMarried = true;

        EventBus.EmitNotification("结为道缘", $"{(d1?.Name ?? "?")}与{(d2?.Name ?? "?")}正式结为道缘，天地为证，从此携手修仙！");
        return true;
    }

    /// <summary>Give a gift to boost affection. Cost is handled by caller.</summary>
    public bool GiveGift(int companionId, int affectionBoost)
    {
        var c = Get(companionId);
        if (c == null) return false;
        c.Affection = Math.Min(100, c.Affection + affectionBoost);
        return true;
    }

    /// <summary>End a companion relationship.</summary>
    public bool BreakUp(int companionId, List<DiscipleData> allDisciples)
    {
        var c = Get(companionId);
        if (c == null) return false;

        var d1 = allDisciples.FirstOrDefault(d => d.Id == c.DiscipleId1);
        var d2 = allDisciples.FirstOrDefault(d => d.Id == c.DiscipleId2);
        if (d1 != null) { d1.CompanionId = -1; d1.IsMarried = false; d1.Mood = Math.Max(0, d1.Mood - 30); }
        if (d2 != null) { d2.CompanionId = -1; d2.IsMarried = false; d2.Mood = Math.Max(0, d2.Mood - 30); }

        _companions.Remove(c);
        EventBus.EmitNotification("道缘断绝", $"{(d1?.Name ?? "?")}与{(d2?.Name ?? "?")}解除了道缘，从此各自修行。");
        return true;
    }

    /// <summary>Daily processing: affection changes, mood bonuses, dual cultivation synergy.
    /// Returns a dictionary of discipleId -> cultivation bonus multiplier.</summary>
    public Dictionary<int, double> ProcessDaily(List<DiscipleData> allDisciples, int currentDay)
    {
        var bonusMap = new Dictionary<int, double>();

        foreach (var c in _companions)
        {
            var d1 = allDisciples.FirstOrDefault(d => d.Id == c.DiscipleId1);
            var d2 = allDisciples.FirstOrDefault(d => d.Id == c.DiscipleId2);
            if (d1 == null || d2 == null) continue;

            bool sameTask = d1.CurrentTask == d2.CurrentTask && d1.CurrentTask != DiscipleTaskType.Rest;
            bool cultivateTogether = d1.CurrentTask == DiscipleTaskType.Cultivate && d2.CurrentTask == DiscipleTaskType.Cultivate;

            // Affection change
            if (sameTask)
            {
                c.Affection = Math.Min(100, c.Affection + 0.3 + c.Affection / 500.0);
                c.YearsTogether++;
            }
            else if (currentDay - c.LastInteractionDay > 3)
            {
                double decay = c.IsMarried ? 0.1 : 0.3;
                c.Affection = Math.Max(0, c.Affection - decay);
            }
            c.LastInteractionDay = currentDay;

            // Mood bonus from companionship
            double moodBonus = c.MoodBonus;
            d1.Mood = Math.Min(100, d1.Mood + moodBonus);
            d2.Mood = Math.Min(100, d2.Mood + moodBonus);

            // Dual cultivation bonus
            if (cultivateTogether)
            {
                double db = c.DualCultivationBonus;
                if (d1.Realm > d2.Realm)
                { bonusMap[d2.Id] = db * 1.3; bonusMap[d1.Id] = db * 0.7; }
                else if (d2.Realm > d1.Realm)
                { bonusMap[d1.Id] = db * 1.3; bonusMap[d2.Id] = db * 0.7; }
                else
                { bonusMap[d1.Id] = db; bonusMap[d2.Id] = db; }
            }

            // Auto-propose marriage when affection is high enough
            if (!c.IsMarried && c.Affection >= 75 && _rng.NextDouble() < 0.02)
            {
                ProposeMarriage(c.Id, allDisciples);
            }

            // Childbirth: married couples with high affection may have children
            if (c.IsMarried && c.Affection >= 70)
            {
                double birthChance = 0.003 * (c.Affection / 100.0); // 0.3% at 100 affection per day
                if (_rng.NextDouble() < birthChance)
                {
                    // Create child with inherited stats
                    var child = CreateChild(d1, d2);
                    EventBus.EmitChildBorn(child, d1, d2);
                    EventBus.EmitNotification("宗门之喜", $"{(d1.IsMale ? d2.Name : d1.Name)}诞下一子，取名{child.Name}！宗门后继有人！");
                }
            }
        }

        return bonusMap;
    }

    DiscipleData CreateChild(DiscipleData p1, DiscipleData p2)
    {
        // Mother is the female parent
        var mother = p1.IsMale ? p2 : p1;
        var father = p1.IsMale ? p1 : p2;

        bool childIsMale = _rng.Next(2) == 0;
        var child = new DiscipleData
        {
            Name = DiscipleNameTable.GenerateName(childIsMale),
            Age = 0,
            IsMale = childIsMale,
            Talent = ClampStat((father.Talent + mother.Talent) / 2 + _rng.Next(-10, 11)),
            Comprehension = ClampStat((father.Comprehension + mother.Comprehension) / 2 + _rng.Next(-10, 11)),
            Constitution = ClampStat((father.Constitution + mother.Constitution) / 2 + _rng.Next(-10, 11)),
            Spirit = ClampStat((father.Spirit + mother.Spirit) / 2 + _rng.Next(-10, 11)),
            Realm = CultivationRealm.Mortal,
            Loyalty = 80, // children are naturally loyal
            Mood = 80,
            CurrentStamina = 120,
            MaxStamina = 120,
            Health = 100,
            MaxHealth = 100,
            // Inherit a skill boost: "道脉传承" skill ID 10
            Skills = new Dictionary<int, int> { { 10, 1 } },
            SpiritRoot = RollChildSpiritRoot(),
        };
        return child;
    }

    SpiritualRoot RollChildSpiritRoot()
    {
        double roll = _rng.NextDouble();
        if (roll < 0.02) return SpiritualRoot.Heavenly;
        if (roll < 0.08) return SpiritualRoot.SingleElement;
        if (roll < 0.20) return SpiritualRoot.DualElement;
        if (roll < 0.50) return SpiritualRoot.ThreeElement;
        if (roll < 0.52) return SpiritualRoot.Special;
        return SpiritualRoot.None;
    }

    static int ClampStat(int v) => Math.Clamp(v, 5, 100);

    public void RemoveCompanion(int id) => _companions.RemoveAll(c => c.Id == id);

    public CompanionData? Get(int id) => _companions.FirstOrDefault(c => c.Id == id);

    public CompanionData? GetByDisciple(int discipleId) =>
        _companions.FirstOrDefault(c => c.DiscipleId1 == discipleId || c.DiscipleId2 == discipleId);

    /// <summary>Calculate compatibility between two disciples (0.0-1.0).</summary>
    private double CalculateCompatibility(DiscipleData d1, DiscipleData d2)
    {
        double score = 0.5; // base 50% chance

        // Similar realm bonus
        if (d1.Realm == d2.Realm) score += 0.15;
        else if (Math.Abs(d1.Realm - d2.Realm) == 1) score += 0.05;

        // Stat synergy: complementary pairs work well
        double statDiff = Math.Abs(d1.Talent - d2.Talent)
                        + Math.Abs(d1.Comprehension - d2.Comprehension)
                        + Math.Abs(d1.Constitution - d2.Constitution)
                        + Math.Abs(d1.Spirit - d2.Spirit);
        score += Math.Max(0, 0.15 - statDiff / 800.0); // closer stats = better

        // Age similarity
        double ageDiff = Math.Abs(d1.Age - d2.Age);
        score += Math.Max(0, 0.10 - ageDiff / 300.0);

        // Loyalty bonus
        score += (d1.Loyalty + d2.Loyalty) / 2000.0;

        return Math.Clamp(score, 0.1, 0.95);
    }

    /// <summary>Random chance for two single disciples to form a companion bond naturally.</summary>
    public void TryRandomPairing(List<DiscipleData> allDisciples)
    {
        // ~2% base chance per day
        if (_rng.NextDouble() > 0.02) return;

        var singleMales = allDisciples.Where(d => d.CompanionId < 0 && d.IsMale && !d.IsMarried).ToList();
        var singleFemales = allDisciples.Where(d => d.CompanionId < 0 && !d.IsMale && !d.IsMarried).ToList();
        if (singleMales.Count == 0 || singleFemales.Count == 0) return;

        var d1 = singleMales[_rng.Next(singleMales.Count)];
        var d2 = singleFemales[_rng.Next(singleFemales.Count)];

        double compat = CalculateCompatibility(d1, d2);
        if (_rng.NextDouble() > compat * 0.7) return; // lower threshold for natural pairing

        var companion = new CompanionData
        {
            Id = _nextId++,
            DiscipleId1 = d1.Id,
            DiscipleId2 = d2.Id,
            Affection = 20 + _rng.NextDouble() * 15, // 20-35 starting affection
        };
        d1.CompanionId = companion.Id;
        d2.CompanionId = companion.Id;
        _companions.Add(companion);
        EventBus.EmitNotification("道缘初结", $"{d1.Name}与{d2.Name}朝夕相处，日久生情，结下了道缘！");
    }

    public void LoadState(List<CompanionData> companions)
    {
        _companions.Clear();
        _companions.AddRange(companions);
        _nextId = _companions.Count > 0 ? _companions.Max(c => c.Id) + 1 : 1;
    }
}
