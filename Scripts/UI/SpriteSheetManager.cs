namespace SwordFateCultivationRecord;

using System.Text.Json;

public static class SpriteSheetManager
{
    private static Texture2D? _atlas;
    private static Dictionary<string, Rect2I> _frames = new();

    private static readonly Dictionary<ResourceType, string> ResourceNames = new()
    {
        { ResourceType.SpiritStone,    "灵石" },
        { ResourceType.Herb,           "灵草" },
        { ResourceType.Ore,            "矿石" },
        { ResourceType.Pill,           "丹药" },
        { ResourceType.Equipment,      "法器" },
        { ResourceType.Contribution,   "贡献" },
        { ResourceType.SpiritEssence,  "灵气" },
    };

    private static readonly Dictionary<FacilityType, string> FacilityNames = new()
    {
        { FacilityType.MeditationChamber, "静修室" },
        { FacilityType.AlchemyRoom,       "丹房" },
        { FacilityType.TrainingGround,    "演武场" },
        { FacilityType.Library,           "藏经阁" },
        { FacilityType.PillRefinery,      "炼药房" },
        { FacilityType.SpiritGarden,      "灵田" },
        { FacilityType.OreMine,           "矿脉" },
        { FacilityType.FormationHall,     "阵法殿" },
        { FacilityType.DiningHall,        "膳堂" },
        { FacilityType.GuestHall,         "会客厅" },
    };

    static SpriteSheetManager()
    {
        LoadAtlas();
    }

    private static void LoadAtlas()
    {
        const string jsonPath = "res://Resources/Textures/sprite_sheet.json";
        const string pngPath  = "res://Resources/Textures/sprite_sheet.png";

        if (!FileAccess.FileExists(jsonPath)) return;

        using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
        if (file == null) return;
        string jsonText = file.GetAsText();
        file.Close();

        using var doc = JsonDocument.Parse(jsonText);
        var framesNode = doc.RootElement.GetProperty("frames");

        _frames = new Dictionary<string, Rect2I>();
        foreach (var frame in framesNode.EnumerateObject())
        {
            var f = frame.Value.GetProperty("frame");
            _frames[frame.Name] = new Rect2I(
                f.GetProperty("x").GetInt32(),
                f.GetProperty("y").GetInt32(),
                f.GetProperty("w").GetInt32(),
                f.GetProperty("h").GetInt32()
            );
        }

        if (ResourceLoader.Exists(pngPath))
            _atlas = ResourceLoader.Load<Texture2D>(pngPath);
    }

    public static AtlasTexture? GetIcon(string name)
    {
        if (_atlas == null || !_frames.TryGetValue(name, out var rect))
            return null;
        return new AtlasTexture { Atlas = _atlas, Region = rect };
    }

    public static AtlasTexture? GetResourceIcon(ResourceType type) =>
        ResourceNames.TryGetValue(type, out var n) ? GetIcon(n) : null;

    public static AtlasTexture? GetFacilityIcon(FacilityType type) =>
        FacilityNames.TryGetValue(type, out var n) ? GetIcon(n) : null;

    public static AtlasTexture? GetAvatar(bool isMale) =>
        GetIcon(isMale ? "男头像" : "女头像");
}
