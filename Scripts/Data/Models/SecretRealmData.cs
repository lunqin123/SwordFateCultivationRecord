namespace SwordFateCultivationRecord;

public class RealmRoom
{
    public string Description { get; set; } = "";
    public string ImageHint { get; set; } = ""; // 场景提示 emoji
    public int DangerLevel { get; set; } = 1; // 1-10
    public List<RealmChoice> Choices { get; set; } = new();
}

public class RealmChoice
{
    public string Text { get; set; } = "";
    public string SuccessText { get; set; } = "";
    public string FailText { get; set; } = "";
    public Dictionary<ResourceType, int> SuccessResources { get; set; } = new();
    public Dictionary<ResourceType, int> FailResources { get; set; } = new();
    public int SuccessReputation { get; set; }
    public int FailReputation { get; set; }
    public int SuccessCultivation { get; set; }
    public int FailHealth { get; set; }
    public int RiskLevel { get; set; } = 1; // 1=安全 2=冒险 3=极度危险
}

public class SecretRealmState
{
    public bool IsActive { get; set; }
    public int CurrentRoom { get; set; }
    public int TotalRooms { get; set; }
    public List<RealmRoom> Rooms { get; set; } = new();
    public int TreasureScore { get; set; }
    public int DamageTaken { get; set; }
    public string RealmName { get; set; } = "";
}
