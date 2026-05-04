using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwordFateCultivationRecord;

public class SaveLoadManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new Vector2IConverter() },
    };

    public const int MaxSlots = 10;
    private static readonly string SaveDir = "user://saves/";

    public bool SaveToSlot(int slotIndex, SaveGameData data)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return false;

        data.SaveTime = DateTime.Now;
        string json = JsonSerializer.Serialize(data, JsonOptions);

        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(SaveDir));

        string path = $"{SaveDir}slot_{slotIndex}.sav";
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null) return false;

        file.StoreString(json);
        return true;
    }

    public SaveGameData? LoadFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return null;

        string path = $"{SaveDir}slot_{slotIndex}.sav";
        if (!FileAccess.FileExists(path)) return null;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return null;

        string json = file.GetAsText();
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<SaveGameData>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return false;
        string path = $"{SaveDir}slot_{slotIndex}.sav";
        if (!FileAccess.FileExists(path)) return false;

        return DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path)) == Error.Ok;
    }

    public int[] GetOccupiedSlots()
    {
        var slots = new List<int>();
        if (!DirAccess.DirExistsAbsolute(ProjectSettings.GlobalizePath(SaveDir)))
            return Array.Empty<int>();

        using var dir = DirAccess.Open(SaveDir);
        if (dir == null) return Array.Empty<int>();

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (fileName.StartsWith("slot_") && fileName.EndsWith(".sav"))
            {
                if (int.TryParse(fileName.Replace("slot_", "").Replace(".sav", ""), out int idx))
                    slots.Add(idx);
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        return slots.ToArray();
    }

    public SaveGameData CreateSaveData(GameManager gm)
    {
        var data = new SaveGameData
        {
            SaveName = $"第{gm.Time.Year}年{gm.Time.Month}月存档",
            CurrentYear = gm.Time.Year,
            CurrentMonth = gm.Time.Month,
            CurrentDay = gm.Time.Day,
            SectName = gm.SectName,
            SectLevel = gm.SectLevel,
            SectReputation = gm.SectReputation,
            SectPower = gm.SectPower,
            MaxDisciples = gm.MaxDisciples,
            OuterDiscipleCount = gm.OuterDiscipleCount,
            Resources = gm.Resources.GetAllResources(),
            IncomePerDay = gm.Resources.GetAllIncome(),
            Disciples = gm.Disciples.AllDisciples.ToList(),
            Facilities = gm.Facilities.AllFacilities.ToList(),
            EventCooldowns = gm.Events.ExportCooldowns(),
            Companions = gm.Companions.AllCompanions.ToList(),
            Equipment = gm.AllEquipment.ToList(),
            EventLog = gm.EventLogEntries.ToList(),
            PlotProgress = gm.Plot.ExportProgress(),
        };
        return data;
    }

    public void ApplySaveData(SaveGameData data, GameManager gm)
    {
        gm.Time.LoadState(data.CurrentYear, data.CurrentMonth, data.CurrentDay);
        gm.SectName = data.SectName;
        gm.SectLevel = data.SectLevel;
        gm.SectReputation = data.SectReputation;
        gm.SectPower = data.SectPower;
        gm.MaxDisciples = data.MaxDisciples;
        gm.OuterDiscipleCount = data.OuterDiscipleCount;
        gm.Resources.LoadState(data.Resources, data.IncomePerDay);
        gm.Disciples.LoadState(data.Disciples);
        gm.Facilities.LoadState(data.Facilities);
        gm.Events.LoadState(data.EventCooldowns);
        gm.Companions.LoadState(data.Companions ?? new List<CompanionData>());
        gm.AllEquipment.Clear();
        if (data.Equipment != null) gm.AllEquipment.AddRange(data.Equipment);
        gm.EventLogEntries.Clear();
        if (data.EventLog != null) gm.EventLogEntries.AddRange(data.EventLog);
        gm.Plot.LoadProgress(data.PlotProgress ?? new PlotProgress());
    }
}

internal class Vector2IConverter : JsonConverter<Vector2I>
{
    public override Vector2I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        int x = 0, y = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string prop = reader.GetString()!;
                reader.Read();
                int val = reader.GetInt32();
                if (prop == "X") x = val;
                else if (prop == "Y") y = val;
            }
        }
        return new Vector2I(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2I value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}
