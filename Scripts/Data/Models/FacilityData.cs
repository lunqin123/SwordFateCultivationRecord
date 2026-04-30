using Godot;

namespace SwordFateCultivationRecord;

public class FacilityData
{
    public int Id { get; set; }
    public FacilityType Type { get; set; }
    public int Level { get; set; } = 1;
    public int MaxLevel { get; set; } = 10;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsBuilt { get; set; }
    public bool IsUnderConstruction { get; set; }
    public bool IsUpgrading { get; set; }
    public int ConstructionProgress { get; set; }
    public int ConstructionCost { get; set; }
    public int UpgradeCostBase { get; set; }
    public int MaxDisciples { get; set; } = 3;
    public Vector2I GridPosition { get; set; }

    public string TypeName => Type switch
    {
        FacilityType.MeditationChamber => "静修室",
        FacilityType.AlchemyRoom => "丹房",
        FacilityType.TrainingGround => "演武场",
        FacilityType.Library => "藏经阁",
        FacilityType.PillRefinery => "炼药房",
        FacilityType.SpiritGarden => "灵田",
        FacilityType.OreMine => "矿脉",
        FacilityType.FormationHall => "阵法殿",
        FacilityType.DiningHall => "膳堂",
        FacilityType.GuestHall => "会客厅",
        _ => "未知"
    };

    public string StatusText => IsUnderConstruction && IsUpgrading ? $"升级中 Lv.{Level}→{Level + 1}"
        : IsUnderConstruction ? "建设中"
        : !IsBuilt ? "未建"
        : "已建成";
}
