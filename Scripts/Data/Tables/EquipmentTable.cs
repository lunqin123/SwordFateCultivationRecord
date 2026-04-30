namespace SwordFateCultivationRecord;

public static class EquipmentTable
{
    public record EquipmentTemplate(
        string Name,
        EquipmentQuality Quality,
        string Description,
        int TalentBonus = 0,
        int ComprehensionBonus = 0,
        int ConstitutionBonus = 0,
        int SpiritBonus = 0,
        int CombatBonus = 0,
        double CultivationSpeedBonus = 0,
        int CostOre = 2,
        int CostSpiritStone = 0
    );

    private static readonly List<EquipmentTemplate> _templates = new()
    {
        // Common (凡品)
        new("铁剑", EquipmentQuality.Common, "一把普通的铁剑，略有锋芒。", CombatBonus: 5, CostOre: 2),
        new("布甲", EquipmentQuality.Common, "粗布缝制的护甲，聊胜于无。", ConstitutionBonus: 3, CostOre: 2),
        new("静心蒲团", EquipmentQuality.Common, "打坐修炼用的蒲团，略微提升修炼效率。", CultivationSpeedBonus: 0.05, CostOre: 1, CostSpiritStone: 20),
        new("药锄", EquipmentQuality.Common, "采药专用的小锄头。", SpiritBonus: 3, CostOre: 2),
        new("传功玉简", EquipmentQuality.Common, "记录基础功法的玉简。", ComprehensionBonus: 3, CostOre: 1, CostSpiritStone: 30),

        // Uncommon (灵品)
        new("青锋剑", EquipmentQuality.Uncommon, "以灵铁锻造的长剑，剑气逼人。", CombatBonus: 12, TalentBonus: 3, CostOre: 4, CostSpiritStone: 50),
        new("灵丝道袍", EquipmentQuality.Uncommon, "以灵蚕丝织就的道袍，轻盈坚韧。", ConstitutionBonus: 6, SpiritBonus: 4, CostOre: 3, CostSpiritStone: 60),
        new("凝神香炉", EquipmentQuality.Uncommon, "点燃后可凝聚心神，加速修炼。", CultivationSpeedBonus: 0.12, ComprehensionBonus: 5, CostOre: 3, CostSpiritStone: 80),
        new("药王鼎", EquipmentQuality.Uncommon, "小型炼丹炉，提升成丹率。", SpiritBonus: 8, CostOre: 5, CostSpiritStone: 70),
        new("悟道蒲团", EquipmentQuality.Uncommon, "以灵草编织的蒲团，有助于悟道。", ComprehensionBonus: 8, CultivationSpeedBonus: 0.05, CostOre: 3, CostSpiritStone: 100),

        // Rare (宝品)
        new("玄冥剑", EquipmentQuality.Rare, "以玄冥铁铸造的宝剑，蕴含冰寒之力。", CombatBonus: 20, TalentBonus: 5, ConstitutionBonus: 3, CostOre: 8, CostSpiritStone: 200),
        new("五行道袍", EquipmentQuality.Rare, "以五行灵丝织就，能调和五行灵气。", ConstitutionBonus: 10, SpiritBonus: 8, ComprehensionBonus: 5, CostOre: 6, CostSpiritStone: 250),
        new("九转丹炉", EquipmentQuality.Rare, "可炼制上品丹药的丹炉。", SpiritBonus: 12, CultivationSpeedBonus: 0.08, CostOre: 8, CostSpiritStone: 300),
        new("灵脉护符", EquipmentQuality.Rare, "以灵石精炼的护符，大幅提升修炼速度。", CultivationSpeedBonus: 0.20, TalentBonus: 8, CostOre: 5, CostSpiritStone: 350),

        // Epic (仙品)
        new("诛仙剑", EquipmentQuality.Epic, "传说中可斩仙人的神剑，锋芒毕露。", CombatBonus: 35, TalentBonus: 10, ConstitutionBonus: 8, CostOre: 15, CostSpiritStone: 800),
        new("太极道袍", EquipmentQuality.Epic, "以太极之理编织的道袍，万法不侵。", ConstitutionBonus: 18, SpiritBonus: 15, ComprehensionBonus: 10, CostOre: 12, CostSpiritStone: 1000),
        new("混元丹炉", EquipmentQuality.Epic, "上古流传的炼丹至宝。", SpiritBonus: 20, CultivationSpeedBonus: 0.15, TalentBonus: 8, CostOre: 15, CostSpiritStone: 1200),
        new("天机盘", EquipmentQuality.Epic, "可窥探天机的至宝，极大提升悟性。", ComprehensionBonus: 20, CultivationSpeedBonus: 0.25, CostOre: 12, CostSpiritStone: 1500),
    };

    private static int _nextId = 100;

    public static EquipmentData CraftRandom(int sectLevel)
    {
        // Higher sect level unlocks better quality chances
        double epicChance = Math.Min(0.20, sectLevel * 0.03);
        double rareChance = Math.Min(0.40, sectLevel * 0.06);
        double uncommonChance = Math.Min(0.60, sectLevel * 0.10);

        var available = _templates.ToList();
        double roll = Random.Shared.NextDouble();
        EquipmentQuality targetQ;
        if (roll < epicChance) targetQ = EquipmentQuality.Epic;
        else if (roll < epicChance + rareChance) targetQ = EquipmentQuality.Rare;
        else if (roll < epicChance + rareChance + uncommonChance) targetQ = EquipmentQuality.Uncommon;
        else targetQ = EquipmentQuality.Common;

        var candidates = available.Where(t => t.Quality == targetQ).ToList();
        if (candidates.Count == 0)
            candidates = available.Where(t => t.Quality <= targetQ).OrderByDescending(t => t.Quality).ToList();
        if (candidates.Count == 0) candidates = available;

        var template = candidates[Random.Shared.Next(candidates.Count)];
        return new EquipmentData
        {
            Id = _nextId++,
            Name = template.Name,
            Quality = template.Quality,
            Description = template.Description,
            TalentBonus = template.TalentBonus,
            ComprehensionBonus = template.ComprehensionBonus,
            ConstitutionBonus = template.ConstitutionBonus,
            SpiritBonus = template.SpiritBonus,
            CombatBonus = template.CombatBonus,
            CultivationSpeedBonus = template.CultivationSpeedBonus,
        };
    }

    public static List<EquipmentTemplate> GetCraftable(int sectLevel)
    {
        return _templates.Where(t => sectLevel >= (t.Quality switch
        {
            EquipmentQuality.Epic => 5,
            EquipmentQuality.Rare => 3,
            _ => 1
        })).ToList();
    }
}
