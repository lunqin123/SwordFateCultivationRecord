namespace SwordFateCultivationRecord;

public class CompanionData
{
    public int Id { get; set; }
    public int DiscipleId1 { get; set; }
    public int DiscipleId2 { get; set; }
    public double Affection { get; set; } = 30;      // 0-100
    public bool IsMarried { get; set; }              // 正式结为道侣
    public int YearsTogether { get; set; }
    public int LastInteractionDay { get; set; }       // absolute day count
    public int RivalId { get; set; } = -1;            // 情敌 discipleId

    public string AffectionLabel => Affection switch
    {
        >= 95 => "神魂交融",
        >= 80 => "情投意合",
        >= 60 => "互生好感",
        >= 30 => "略有好感",
        _ => "萍水相逢"
    };

    public double DualCultivationBonus => IsMarried
        ? 0.15 + Affection / 100.0 * 0.35   // married: 15-50%
        : 0.05 + Affection / 100.0 * 0.15;  // not married: 5-20%

    public double MoodBonus => Affection / 200.0; // 0-0.5 mood boost per day when together
}
