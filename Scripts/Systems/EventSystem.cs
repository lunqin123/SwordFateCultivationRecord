namespace SwordFateCultivationRecord;

public class EventSystem
{
    private readonly Random _rng = new();
    private readonly Dictionary<int, int> _cooldowns = new(); // eventId -> remaining days

    public const double DailyEventChance = 0.15; // 15%

    public void ProcessCooldowns()
    {
        var expired = new List<int>();
        foreach (var kv in _cooldowns)
        {
            _cooldowns[kv.Key] = kv.Value - 1;
            if (_cooldowns[kv.Key] <= 0)
                expired.Add(kv.Key);
        }
        foreach (var id in expired)
            _cooldowns.Remove(id);
    }

    public EventData? TryTriggerEvent(int sectLevel)
    {
        if (_rng.NextDouble() >= DailyEventChance) return null;

        var available = EventTable.GetAvailableEvents(sectLevel)
            .Where(e => !_cooldowns.ContainsKey(e.Id))
            .ToList();

        if (available.Count == 0) return null;

        int totalWeight = available.Sum(e => e.Weight);
        int roll = _rng.Next(totalWeight);
        int cumulative = 0;

        foreach (var e in available)
        {
            cumulative += e.Weight;
            if (roll < cumulative)
            {
                _cooldowns[e.Id] = e.CooldownDays > 0 ? e.CooldownDays : 30;
                return e;
            }
        }

        return null;
    }

    public void ApplyOutcome(EventData eventData, int choiceIndex, ResourceSystem resources,
        DiscipleSystem discipleSystem, ref int reputation, ref int power)
    {
        var outcome = choiceIndex switch
        {
            0 => eventData.Choice1Outcome,
            1 => eventData.Choice2Outcome,
            2 => eventData.Choice3Outcome,
            _ => null
        };

        if (outcome == null) return;

        // Apply resource changes
        if (outcome.ResourceChanges != null)
        {
            foreach (var kv in outcome.ResourceChanges)
            {
                if (kv.Value > 0)
                    resources.Add(kv.Key, kv.Value);
                else
                    resources.Spend(kv.Key, -kv.Value);
            }
        }

        // Apply reputation and power changes
        reputation += outcome.ReputationChange;
        power += outcome.PowerChange;

        // Apply disciple stat effects (to all disciples)
        if (outcome.DiscipleStatEffects != null)
        {
            foreach (var d in discipleSystem.AllDisciples)
            {
                d.Loyalty = Math.Clamp(d.Loyalty + outcome.DiscipleStatEffects.LoyaltyChange, 0, 100);
                d.Mood = Math.Clamp(d.Mood + outcome.DiscipleStatEffects.MoodChange, 0, 100);
                d.Health = Math.Clamp(d.Health + outcome.DiscipleStatEffects.HealthChange, 1, d.MaxHealth);
            }
        }

        // Apply cultivation bonus (to all disciples)
        if (outcome.DiscipleCultivationBonus > 0)
        {
            foreach (var d in discipleSystem.AllDisciples)
            {
                d.CultivationProgress += outcome.DiscipleCultivationBonus;
            }
        }

        EventBus.EmitNotification(eventData.Title, outcome.ResultText);
    }

    public void LoadState(Dictionary<int, int>? cooldowns)
    {
        _cooldowns.Clear();
        if (cooldowns != null)
        {
            foreach (var kv in cooldowns)
                _cooldowns[kv.Key] = kv.Value;
        }
    }

    public Dictionary<int, int> ExportCooldowns() => new(_cooldowns);
}
