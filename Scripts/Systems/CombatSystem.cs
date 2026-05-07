namespace SwordFateCultivationRecord;

public class CombatSystem
{
    private Random _rng => Random.Shared;

    private static readonly List<CombatMission> _allMissions = new()
    {
        new()
        {
            MissionId = 1, Name = "山贼剿灭", EnemyName = "山贼团伙",
            Description = "附近山头的山贼骚扰凡人村落，宗门当替天行道。",
            EnemyPower = 30, MinSectLevel = 1,
            VictoryRewards = new() { [ResourceType.SpiritStone] = 50, [ResourceType.Ore] = 10 },
            VictoryReputation = 5, FailureReputation = -2,
            EquipmentDropChance = 0.05, InjuryAmount = 8, MaxDisciples = 3,
        },
        new()
        {
            MissionId = 2, Name = "妖兽驱逐", EnemyName = "嗜血妖兽",
            Description = "一头妖兽在宗门附近游荡，威胁弟子安全。",
            EnemyPower = 60, MinSectLevel = 1,
            VictoryRewards = new() { [ResourceType.SpiritStone] = 80, [ResourceType.Herb] = 20, [ResourceType.Ore] = 15 },
            VictoryReputation = 8, FailureReputation = -3,
            EquipmentDropChance = 0.10, InjuryAmount = 10, MaxDisciples = 4,
        },
        new()
        {
            MissionId = 3, Name = "邪修巢穴", EnemyName = "邪修",
            Description = "探子来报，有邪修在山中建立巢穴，恐荼毒生灵。",
            EnemyPower = 120, MinSectLevel = 2,
            VictoryRewards = new() { [ResourceType.SpiritStone] = 150, [ResourceType.Pill] = 10, [ResourceType.SpiritEssence] = 15 },
            VictoryReputation = 12, FailureReputation = -5,
            EquipmentDropChance = 0.15, InjuryAmount = 12, MaxDisciples = 5,
        },
        new()
        {
            MissionId = 4, Name = "上古遗迹", EnemyName = "遗迹守卫傀儡",
            Description = "发现一处上古修士的遗迹，守护傀儡仍在运转。",
            EnemyPower = 200, MinSectLevel = 3,
            VictoryRewards = new() { [ResourceType.SpiritStone] = 250, [ResourceType.Equipment] = 3, [ResourceType.SpiritEssence] = 30 },
            VictoryReputation = 18, FailureReputation = -8,
            EquipmentDropChance = 0.25, InjuryAmount = 15, MaxDisciples = 5,
        },
        new()
        {
            MissionId = 5, Name = "魔教据点", EnemyName = "魔教护法",
            Description = "魔教在边境建立了据点，意图染指中原修真界。",
            EnemyPower = 350, MinSectLevel = 4,
            VictoryRewards = new() { [ResourceType.SpiritStone] = 400, [ResourceType.Pill] = 20, [ResourceType.Ore] = 50, [ResourceType.SpiritEssence] = 40 },
            VictoryReputation = 25, FailureReputation = -12,
            EquipmentDropChance = 0.35, InjuryAmount = 18, MaxDisciples = 5,
        },
        new()
        {
            MissionId = 6, Name = "天外战场", EnemyName = "域外天魔",
            Description = "天外裂缝出现，域外天魔蠢蠢欲动。此战关乎苍生。",
            EnemyPower = 550, MinSectLevel = 5,
            VictoryRewards = new() { [ResourceType.SpiritStone] = 600, [ResourceType.Equipment] = 5, [ResourceType.Pill] = 30, [ResourceType.SpiritEssence] = 60 },
            VictoryReputation = 40, FailureReputation = -20,
            EquipmentDropChance = 0.50, InjuryAmount = 22, MaxDisciples = 5,
        },
        new()
        {
            MissionId = 7, Name = "虚空裂隙", EnemyName = "虚空巨兽",
            Description = "虚空深处有巨兽苏醒，一旦降临世间，万物皆休。",
            EnemyPower = 800, MinSectLevel = 6,
            VictoryRewards = new() { [ResourceType.SpiritStone] = 1000, [ResourceType.Equipment] = 8, [ResourceType.Pill] = 50, [ResourceType.SpiritEssence] = 100 },
            VictoryReputation = 60, FailureReputation = -30,
            EquipmentDropChance = 0.65, InjuryAmount = 28, MaxDisciples = 5,
        },
    };

    /// <summary>Get missions unlocked at the given sect level.</summary>
    public List<CombatMission> GetAvailableMissions(int sectLevel)
    {
        return _allMissions.Where(m => m.MinSectLevel <= sectLevel).ToList();
    }

    /// <summary>Resolve a combat mission with the selected party of disciples.</summary>
    public CombatResult Resolve(List<DiscipleData> party, CombatMission mission)
    {
        var result = new CombatResult
        {
            MissionName = mission.Name,
            EnemyName = mission.EnemyName,
        };

        // Calculate party stats
        double partyPower = party.Sum(d => d.CombatPower);
        double partyHP = party.Sum(d => d.EffConstitution * 8 + d.Health * 0.5);
        double enemyPower = mission.EnemyPower;
        double enemyHP = enemyPower * 4;

        // Per-disciple tracking
        var reports = party.Select(d => new DiscipleBattleReport
        {
            Name = d.Name,
            StartingPower = d.CombatPower,
        }).ToList();

        double totalDamageDealt = 0;
        double partyAtkPerRound = Math.Max(1, partyPower / 12);
        double enemyAtkPerRound = Math.Max(1, enemyPower / 12);
        var logLines = new List<string>();
        int rounds = 0;

        logLines.Add($"⚔ 宗门弟子 vs {mission.EnemyName}");
        logLines.Add($"  我方战力 {partyPower:F0}  VS 敌方战力 {enemyPower:F0}");
        logLines.Add("");

        // Combat rounds (max 15)
        while (enemyHP > 0 && partyHP > 0 && rounds < 15)
        {
            rounds++;

            // Party attacks
            double dmg = partyAtkPerRound * (0.7 + _rng.NextDouble() * 0.6);
            enemyHP -= dmg;
            totalDamageDealt += dmg;

            // Spread damage dealt among disciples based on their power share
            double powerShare = party.Sum(d => d.CombatPower);
            for (int i = 0; i < reports.Count; i++)
            {
                double share = powerShare > 0 ? party[i].CombatPower / powerShare : 1.0 / party.Count;
                reports[i].DamageDealt += (int)(dmg * share);
            }

            if (enemyHP <= 0) break;

            // Enemy attacks (target random disciple weighted by power)
            double eDmg = enemyAtkPerRound * (0.7 + _rng.NextDouble() * 0.6);
            double totalMitigated = 0;
            for (int i = 0; i < party.Count; i++)
            {
                // Constitution mitigates damage: 1% per point
                double mitigation = 1.0 - party[i].EffConstitution * 0.008;
                double individualDmg = eDmg / party.Count * Math.Max(0.1, mitigation);
                totalMitigated += individualDmg;
                int hpLoss = Math.Max(1, (int)(individualDmg * 2));
                party[i].Health = Math.Max(1, party[i].Health - hpLoss);
                reports[i].HealthLost += hpLoss;
                partyHP -= individualDmg;
            }
        }

        // Determine outcome
        bool victory = enemyHP <= 0;
        result.Victory = victory;
        result.RoundsFought = rounds;

        if (victory)
        {
            logLines.Add($"✦ 胜利！历经 {rounds} 回合击败了{mission.EnemyName}。");

            // Full rewards
            foreach (var kv in mission.VictoryRewards)
                result.Rewards[kv.Key] = kv.Value;

            result.ReputationGain = mission.VictoryReputation;

            // Equipment drop chance
            if (_rng.NextDouble() < mission.EquipmentDropChance)
            {
                result.EquipmentGained = 1;
                logLines.Add("  缴获法器一件！");
            }

            // Minor injuries regardless
            foreach (var d in party)
            {
                int minInjury = Math.Max(1, mission.InjuryAmount / 3);
                d.Health = Math.Max(1, d.Health - _rng.Next(0, minInjury + 1));
            }
        }
        else
        {
            logLines.Add($"✗ 败退… 经过 {rounds} 回合苦战，弟子们不敌{mission.EnemyName}。");

            // Partial rewards (40%)
            foreach (var kv in mission.VictoryRewards)
                result.Rewards[kv.Key] = Math.Max(1, kv.Value * 40 / 100);

            result.ReputationGain = mission.FailureReputation;

            // Injuries
            foreach (var d in party)
            {
                int injury = _rng.Next(mission.InjuryAmount / 2, mission.InjuryAmount + 1);
                d.Health = Math.Max(1, d.Health - injury);
            }
        }

        // Mark injured disciples
        foreach (var r in reports)
            r.WasInjured = r.HealthLost > 10;

        result.TotalDamageTaken = reports.Sum(r => r.HealthLost);
        result.BattleLog = string.Join("\n", logLines);
        result.DiscipleReports = reports;

        return result;
    }
}
