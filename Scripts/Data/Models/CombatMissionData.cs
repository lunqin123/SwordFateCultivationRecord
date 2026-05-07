namespace SwordFateCultivationRecord;

/// <summary>Combat mission template — defines a battle the player can send disciples on.</summary>
public class CombatMission
{
    public int MissionId { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string EnemyName { get; init; } = "";
    public int EnemyPower { get; init; }          // Difficulty rating
    public int MinSectLevel { get; init; } = 1;   // Required sect level to unlock
    public Dictionary<ResourceType, int> VictoryRewards { get; init; } = new();
    public int VictoryReputation { get; init; }
    public int FailureReputation { get; init; }
    public double EquipmentDropChance { get; init; } // Chance to drop equipment on victory
    public int InjuryAmount { get; init; } = 10;   // Health lost per disciple on failure
    public int MaxDisciples { get; init; } = 5;     // Max disciples that can join
}

/// <summary>Result of a resolved combat mission.</summary>
public class CombatResult
{
    public bool Victory { get; set; }
    public int RoundsFought { get; set; }
    public Dictionary<ResourceType, int> Rewards { get; set; } = new();
    public int ReputationGain { get; set; }
    public int EquipmentGained { get; set; }
    public string BattleLog { get; set; } = "";
    public int TotalDamageTaken { get; set; }
    public List<DiscipleBattleReport> DiscipleReports { get; set; } = new();
    public string EnemyName { get; set; } = "";
    public string MissionName { get; set; } = "";
}

/// <summary>Per-disciple battle result.</summary>
public class DiscipleBattleReport
{
    public string Name { get; set; } = "";
    public int HealthLost { get; set; }
    public int DamageDealt { get; set; }
    public int StartingPower { get; set; }
    public bool WasInjured { get; set; }
}
