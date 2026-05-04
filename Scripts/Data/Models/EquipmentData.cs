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
    public int EquippedById { get; set; } = -1;

    public string QualityName => Quality switch
    {
        EquipmentQuality.Common => "凡品",
        EquipmentQuality.Uncommon => "灵品",
        EquipmentQuality.Rare => "宝品",
        EquipmentQuality.Epic => "仙品",
        _ => "未知"
    };

    public string FullName => $"[{QualityName}] {Name}";

    public void UpgradeQuality()
    {
        if (Quality >= EquipmentQuality.Epic) return;
        Quality = (EquipmentQuality)((int)Quality + 1);
        double mult = Quality switch { EquipmentQuality.Uncommon => 1.5, EquipmentQuality.Rare => 2.5, EquipmentQuality.Epic => 4.0, _ => 1.0 };
        double prevMult = Quality == EquipmentQuality.Uncommon ? 1.0 : Quality == EquipmentQuality.Rare ? 1.5 : 2.5;
        double scale = mult / prevMult;
        TalentBonus = Math.Max(1, (int)(TalentBonus * scale));
        ComprehensionBonus = Math.Max(1, (int)(ComprehensionBonus * scale));
        ConstitutionBonus = Math.Max(1, (int)(ConstitutionBonus * scale));
        SpiritBonus = Math.Max(1, (int)(SpiritBonus * scale));
        CombatBonus = Math.Max(1, (int)(CombatBonus * scale));
        CultivationSpeedBonus = Math.Max(0.01, CultivationSpeedBonus * scale);
    }
}
