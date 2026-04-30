namespace SwordFateCultivationRecord;

public class TimeSystem
{
    public int Year { get; private set; } = 1;
    public int Month { get; private set; } = 1;
    public int Day { get; private set; } = 1;

    public const int DaysPerMonth = 30;
    public const int MonthsPerYear = 12;

    /// <summary>Advance by one day, emit DayPassed/ MonthPassed/ YearPassed.</summary>
    public void AdvanceDay()
    {
        Day++;
        if (Day > DaysPerMonth)
        {
            Day = 1;
            Month++;
            if (Month > MonthsPerYear)
            {
                Month = 1;
                Year++;
                EventBus.EmitYearPassed(Year);
            }
            EventBus.EmitMonthPassed(Month, Year);
        }
        EventBus.EmitDayPassed(Day, Month, Year);
    }

    /// <summary>Advance multiple days at once, each triggers events.</summary>
    public void AdvanceDays(int days)
    {
        for (int i = 0; i < days; i++)
            AdvanceDay();
    }

    public int GetTotalDays() => (Year - 1) * DaysPerMonth * MonthsPerYear + (Month - 1) * DaysPerMonth + Day;
    public string GetDateString() => $"第{Year}年{Month}月{Day}日";

    public void LoadState(int year, int month, int day)
    {
        Year = year;
        Month = month;
        Day = day;
    }
}
