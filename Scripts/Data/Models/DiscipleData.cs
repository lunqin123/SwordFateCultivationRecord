namespace SwordFateCultivationRecord;

public enum SpiritualRoot { None, ThreeElement, DualElement, SingleElement, Heavenly, Special }

public class DiscipleData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; } = 16;
    public bool IsMale { get; set; }
    public SpiritualRoot SpiritRoot { get; set; } = SpiritualRoot.None;
    public string AvatarPath { get; set; } = ""; // 头像资源路径，留空用默认

    public string SpiritRootName => SpiritRoot switch
    {
        SpiritualRoot.None => "杂灵根",
        SpiritualRoot.ThreeElement => "三灵根",
        SpiritualRoot.DualElement => "双灵根",
        SpiritualRoot.SingleElement => "单灵根",
        SpiritualRoot.Heavenly => "天灵根",
        SpiritualRoot.Special => "异灵根",
        _ => "未知"
    };

    public double SpiritRootCultivationBonus => SpiritRoot switch
    {
        SpiritualRoot.None => 0,
        SpiritualRoot.ThreeElement => 0.15,
        SpiritualRoot.DualElement => 0.30,
        SpiritualRoot.SingleElement => 0.60,
        SpiritualRoot.Heavenly => 1.20,
        SpiritualRoot.Special => 0.40,
        _ => 0
    };

    // Core stats (0-100)
    public int Talent { get; set; }          // 天赋 - affects cultivation speed
    public int Comprehension { get; set; }   // 悟性 - affects skill learning
    public int Constitution { get; set; }    // 体质 - affects HP/stamina
    public int Spirit { get; set; }          // 神识 - affects alchemy/craft
    public int Loyalty { get; set; } = 50;   // 忠心 - affects leave chance

    // Unique flavor attributes
    public string Background { get; set; } = "";   // 身世背景 e.g. "农家子弟"
    public string Personality { get; set; } = "";  // 性格特质 e.g. "沉稳持重"
    public string Trait { get; set; } = "";        // 特殊天赋 e.g. "过目不忘"

    // Cultivation realm
    public CultivationRealm Realm { get; set; } = CultivationRealm.Mortal;
    public int RealmLayer { get; set; }            // 1-9 within current realm
    public double CultivationProgress { get; set; }
    public double BreakthroughProgress { get; set; }
    public bool IsInBreakthrough { get; set; }

    // Status
    public int CurrentStamina { get; set; } = 120;
    public int MaxStamina { get; set; } = 120;
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public double Mood { get; set; } = 50.0;       // 心情 0-100
    public int YearsInSect { get; set; }

    // Companion
    public int CompanionId { get; set; } = -1;       // -1 = single
    public bool IsMarried { get; set; }

    // Current task
    public DiscipleTaskType CurrentTask { get; set; } = DiscipleTaskType.Rest;
    public int TaskTargetId { get; set; } = -1;

    // Skills and equipment
    public Dictionary<int, int> Skills { get; set; } = new();
    public List<int> EquipmentIds { get; set; } = new();

    public int TotalContribution { get; set; }

    public int ConsecutiveLowMoodDays { get; set; }
    public Dictionary<DiscipleTaskType, int> TaskProficiency { get; set; } = new(); // task type -> level (0-5)

    public double TaskProgress { get; set; }

    public int CombatPower => CalculateCombatPower();

    private int CalculateCombatPower()
    {
        int basePower = (int)(((int)Realm * 10 + RealmLayer) * 100
               + Talent * 2 + Comprehension * 1.5 + Constitution * 3);
        int equipBonus = 0;
        if (EquipmentBonusCache != null)
        {
            equipBonus = EquipmentBonusCache.CombatBonus
                + EquipmentBonusCache.ConstitutionBonus * 3
                + EquipmentBonusCache.TalentBonus * 2;
        }
        return basePower + equipBonus;
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public EquipmentBonusCache? EquipmentBonusCache { get; set; }

    // Effective stats including equipment bonuses
    public int EffTalent => Talent + (EquipmentBonusCache?.TalentBonus ?? 0);
    public int EffComprehension => Comprehension + (EquipmentBonusCache?.ComprehensionBonus ?? 0);
    public int EffConstitution => Constitution + (EquipmentBonusCache?.ConstitutionBonus ?? 0);
    public int EffSpirit => Spirit + (EquipmentBonusCache?.SpiritBonus ?? 0);
    public double EffCultivationSpeedBonus => EquipmentBonusCache?.CultivationSpeedBonus ?? 0;

    public string RealmName => Realm switch
    {
        CultivationRealm.Mortal => "凡人",
        CultivationRealm.QiRefining => "练气",
        CultivationRealm.Foundation => "筑基",
        CultivationRealm.CoreFormation => "金丹",
        CultivationRealm.NascentSoul => "元婴",
        CultivationRealm.SpiritTransformation => "化神",
        CultivationRealm.Tribulation => "渡劫",
        CultivationRealm.GreatAscension => "大乘",
        _ => "未知"
    };

    public string FullRealmName =>
        Realm == CultivationRealm.Mortal ? "凡人" : $"{RealmName}{RealmLayer}层";
}

public class EquipmentBonusCache
{
    public int TalentBonus { get; set; }
    public int ComprehensionBonus { get; set; }
    public int ConstitutionBonus { get; set; }
    public int SpiritBonus { get; set; }
    public int CombatBonus { get; set; }
    public double CultivationSpeedBonus { get; set; }
}
