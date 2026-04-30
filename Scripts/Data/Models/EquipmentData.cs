namespace SwordFateCultivationRecord;

public enum EquipmentQuality { Common, Uncommon, Rare, Epic }

public class EquipmentData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public EquipmentQuality Quality { get; set; } = EquipmentQuality.Common;
    public string Description { get; set; } = "";
    public int TalentBonus { get; set; }
    public int ComprehensionBonus { get; set; }
    public int ConstitutionBonus { get; set; }
    public int SpiritBonus { get; set; }
    public int CombatBonus { get; set; }
    public double CultivationSpeedBonus { get; set; }
    public int EquippedById { get; set; } = -1; // -1 = not equipped

    public string QualityName => Quality switch
    {
        EquipmentQuality.Common => "凡品",
        EquipmentQuality.Uncommon => "灵品",
        EquipmentQuality.Rare => "宝品",
        EquipmentQuality.Epic => "仙品",
        _ => "未知"
    };

    public string FullName => $"[{QualityName}] {Name}";
}
