namespace SwordFateCultivationRecord;

/// <summary>
/// Static event bus for cross-system communication without tight coupling.
/// Replaces Godot signals with type-safe C# events.
/// </summary>
public static class EventBus
{
    // Disciple events
    public static event Action<DiscipleData>? DiscipleRecruited;
    public static event Action<DiscipleData>? DiscipleDeparted;
    public static event Action<DiscipleData>? BreakthroughSuccess;
    public static event Action<DiscipleData>? BreakthroughFailed;
    public static event Action<DiscipleData, DiscipleData, DiscipleData>? ChildBorn; // child, parent1, parent2

    // Notification
    public static event Action<string, string>? GameNotification; // (title, message)
    public static event Action<EventData>? EventChoiceRequired;

    // Resource change (type, oldValue, newValue)
    public static event Action<ResourceType, int, int>? ResourceChanged;

    // Recruit selection — candidates ready for UI to display
    public static event Action<List<DiscipleData>>? RecruitSelectionReady;
    public static event Action<PlotStageDef, string>? PlotStageCompleted;
    public static event Action<PlotStageDef>? PlotStageActivated;
    public static event Action? GameEnding;
    public static event Action<int, int, int>? DayPassed; // (day, month, year)
    public static event Action<int, int>? MonthPassed;     // (month, year)
    public static event Action<int>? YearPassed;           // (year)

    public static void EmitDiscipleRecruited(DiscipleData d) => DiscipleRecruited?.Invoke(d);
    public static void EmitDiscipleDeparted(DiscipleData d) => DiscipleDeparted?.Invoke(d);
    public static void EmitBreakthroughSuccess(DiscipleData d) => BreakthroughSuccess?.Invoke(d);
    public static void EmitBreakthroughFailed(DiscipleData d) => BreakthroughFailed?.Invoke(d);
    public static void EmitChildBorn(DiscipleData child, DiscipleData p1, DiscipleData p2) => ChildBorn?.Invoke(child, p1, p2);
    public static void EmitNotification(string title, string msg) => GameNotification?.Invoke(title, msg);
    public static void EmitEventChoiceRequired(EventData e) => EventChoiceRequired?.Invoke(e);
    public static void EmitResourceChanged(ResourceType t, int old, int n) => ResourceChanged?.Invoke(t, old, n);
    public static void EmitDayPassed(int d, int m, int y) => DayPassed?.Invoke(d, m, y);
    public static void EmitMonthPassed(int m, int y) => MonthPassed?.Invoke(m, y);
    public static void EmitYearPassed(int y) => YearPassed?.Invoke(y);
    public static void EmitRecruitSelectionReady(List<DiscipleData> candidates) => RecruitSelectionReady?.Invoke(candidates);
    public static void EmitPlotStageCompleted(PlotStageDef stage, string message) => PlotStageCompleted?.Invoke(stage, message);
    public static void EmitPlotStageActivated(PlotStageDef stage) => PlotStageActivated?.Invoke(stage);
    public static void EmitGameEnding() => GameEnding?.Invoke();

    public static void Clear()
    {
        DiscipleRecruited = null;
        DiscipleDeparted = null;
        BreakthroughSuccess = null;
        BreakthroughFailed = null;
        ChildBorn = null;
        GameNotification = null;
        EventChoiceRequired = null;
        ResourceChanged = null;
        DayPassed = null;
        MonthPassed = null;
        YearPassed = null;
        RecruitSelectionReady = null;
        PlotStageCompleted = null;
        PlotStageActivated = null;
        GameEnding = null;
    }
}
