namespace SwordFateCultivationRecord;

public class FacilitySystem
{
    private readonly List<FacilityData> _facilities = new();
    private int _nextId = 1000;

    public IReadOnlyList<FacilityData> AllFacilities => _facilities;
    public int Count => _facilities.Count;

    public FacilityData? Build(FacilityType type)
    {
        var info = FacilityTable.GetInfo(type);
        var facility = new FacilityData
        {
            Id = _nextId++,
            Type = type,
            Name = info.Name,
            Description = info.Description,
            IsUnderConstruction = true,
            ConstructionProgress = 0,
            ConstructionCost = info.BaseBuildCost,
            MaxLevel = FacilityTable.GetMaxLevel(),
            MaxDisciples = info.MaxDisciples,
        };
        _facilities.Add(facility);
        return facility;
    }

    public FacilityData? Get(int id) => _facilities.FirstOrDefault(f => f.Id == id);
    public List<FacilityData> GetByType(FacilityType type) => _facilities.Where(f => f.Type == type).ToList();

    public bool StartUpgrade(int facilityId, ResourceSystem resources)
    {
        var f = Get(facilityId);
        if (f == null || !f.IsBuilt || f.Level >= f.MaxLevel) return false;

        f.IsUnderConstruction = true;
        f.IsUpgrading = true;
        f.ConstructionProgress = 0;
        return true;
    }

    public void ProcessDaily(ResourceSystem resources)
    {
        var dailyIncome = new Dictionary<ResourceType, int>();

        foreach (var f in _facilities)
        {
            var facilityInfo = FacilityTable.GetInfo(f.Type);

            if (f.IsUnderConstruction)
            {
                f.ConstructionProgress++;
                if (f.ConstructionProgress >= facilityInfo.BuildDays)
                {
                    f.IsUnderConstruction = false;
                    f.IsBuilt = true;
                    f.ConstructionProgress = 0;
                    if (f.IsUpgrading)
                    {
                        f.Level++;
                        f.IsUpgrading = false;
                        EventBus.EmitNotification("灵筑晋升", $"{f.TypeName}升级至Lv.{f.Level}！");
                    }
                    else
                    {
                        EventBus.EmitNotification("灵筑竣工", $"{f.TypeName}建造完成！");
                    }
                }
                continue;
            }

            if (!f.IsBuilt) continue;

            // Calculate daily output
            int output = FacilityTable.GetOutput(f.Type, f.Level);
            if (output > 0)
            {
                dailyIncome.TryAdd(facilityInfo.OutputType, 0);
                dailyIncome[facilityInfo.OutputType] += output;
            }
        }

        resources.SetDailyIncome(dailyIncome);
    }

    public int GetFacilityBonus(FacilityType type, int level)
    {
        return FacilityTable.GetOutput(type, level);
    }

    public void LoadState(List<FacilityData> facilities)
    {
        _facilities.Clear();
        _facilities.AddRange(facilities);
        _nextId = _facilities.Count > 0 ? _facilities.Max(f => f.Id) + 1 : 1000;
    }
}
