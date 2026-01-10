using Adorika.Domain.Enums;

namespace Adorika.Domain.Extensions;

/// <summary>
/// Extension methods for WorkingDay enum to work with flags.
/// </summary>
public static class WorkingDayExtensions
{
    /// <summary>
    /// Checks if a specific day is included in the working days.
    /// </summary>
    public static bool HasDay(this WorkingDay days, WorkingDay day)
    {
        return (days & day) == day;
    }

    /// <summary>
    /// Adds a day to the working days.
    /// </summary>
    public static WorkingDay AddDay(this WorkingDay days, WorkingDay day)
    {
        return days | day;
    }

    /// <summary>
    /// Removes a day from the working days.
    /// </summary>
    public static WorkingDay RemoveDay(this WorkingDay days, WorkingDay day)
    {
        return days & ~day;
    }

    /// <summary>
    /// Converts a DayOfWeek to WorkingDay enum.
    /// </summary>
    public static WorkingDay ToWorkingDay(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => WorkingDay.Monday,
            DayOfWeek.Tuesday => WorkingDay.Tuesday,
            DayOfWeek.Wednesday => WorkingDay.Wednesday,
            DayOfWeek.Thursday => WorkingDay.Thursday,
            DayOfWeek.Friday => WorkingDay.Friday,
            DayOfWeek.Saturday => WorkingDay.Saturday,
            DayOfWeek.Sunday => WorkingDay.Sunday,
            _ => WorkingDay.None
        };
    }

    /// <summary>
    /// Checks if a specific DayOfWeek is included in the working days.
    /// </summary>
    public static bool IncludesDay(this WorkingDay days, DayOfWeek dayOfWeek)
    {
        var workingDay = dayOfWeek.ToWorkingDay();
        return days.HasDay(workingDay);
    }

    /// <summary>
    /// Gets all individual days from the flags.
    /// </summary>
    public static IEnumerable<WorkingDay> GetDays(this WorkingDay days)
    {
        if (days == WorkingDay.None)
        {
            yield break;
        }

        if (days.HasDay(WorkingDay.Monday))
        {
            yield return WorkingDay.Monday;
        }

        if (days.HasDay(WorkingDay.Tuesday))
        {
            yield return WorkingDay.Tuesday;
        }

        if (days.HasDay(WorkingDay.Wednesday))
        {
            yield return WorkingDay.Wednesday;
        }

        if (days.HasDay(WorkingDay.Thursday))
        {
            yield return WorkingDay.Thursday;
        }

        if (days.HasDay(WorkingDay.Friday))
        {
            yield return WorkingDay.Friday;
        }

        if (days.HasDay(WorkingDay.Saturday))
        {
            yield return WorkingDay.Saturday;
        }

        if (days.HasDay(WorkingDay.Sunday))
        {
            yield return WorkingDay.Sunday;
        }
    }

    /// <summary>
    /// Gets all DayOfWeek values from the working days flags.
    /// </summary>
    public static IEnumerable<DayOfWeek> GetDayOfWeekValues(this WorkingDay days)
    {
        return days.GetDays().Select(d => d switch
        {
            WorkingDay.Monday => DayOfWeek.Monday,
            WorkingDay.Tuesday => DayOfWeek.Tuesday,
            WorkingDay.Wednesday => DayOfWeek.Wednesday,
            WorkingDay.Thursday => DayOfWeek.Thursday,
            WorkingDay.Friday => DayOfWeek.Friday,
            WorkingDay.Saturday => DayOfWeek.Saturday,
            WorkingDay.Sunday => DayOfWeek.Sunday,
            _ => throw new ArgumentOutOfRangeException()
        });
    }

    /// <summary>
    /// Counts how many working days are enabled.
    /// </summary>
    public static int CountDays(this WorkingDay days)
    {
        if (days == WorkingDay.None)
        {
            return 0;
        }

        var count = 0;
        var value = (int)days;

        // Count set bits using Brian Kernighan's algorithm
        while (value != 0)
        {
            value &= value - 1;
            count++;
        }

        return count;
    }

    /// <summary>
    /// Gets a human-readable string of working days.
    /// </summary>
    public static string ToDisplayString(this WorkingDay days)
    {
        if (days == WorkingDay.None)
        {
            return "None";
        }

        var enabledDays = days.GetDays()
            .Select(d => d.ToString())
            .ToArray();

        return string.Join(", ", enabledDays);
    }

    /// <summary>
    /// Common working day patterns.
    /// </summary>
    public static class Patterns
    {
        /// <summary>
        /// Monday to Friday (typical 5-day work week).
        /// </summary>
        public static WorkingDay Weekdays => WorkingDay.Monday | WorkingDay.Tuesday |
                                             WorkingDay.Wednesday | WorkingDay.Thursday |
                                             WorkingDay.Friday;

        /// <summary>
        /// Saturday and Sunday (weekend).
        /// </summary>
        public static WorkingDay Weekend => WorkingDay.Saturday | WorkingDay.Sunday;

        /// <summary>
        /// All days of the week.
        /// </summary>
        public static WorkingDay AllDays => WorkingDay.Monday | WorkingDay.Tuesday |
                                           WorkingDay.Wednesday | WorkingDay.Thursday |
                                           WorkingDay.Friday | WorkingDay.Saturday |
                                           WorkingDay.Sunday;

        /// <summary>
        /// Sunday to Thursday (common in Middle East).
        /// </summary>
        public static WorkingDay SundayToThursday => WorkingDay.Sunday | WorkingDay.Monday |
                                                     WorkingDay.Tuesday | WorkingDay.Wednesday |
                                                     WorkingDay.Thursday;

        /// <summary>
        /// Monday to Saturday (6-day work week).
        /// </summary>
        public static WorkingDay MondayToSaturday => WorkingDay.Monday | WorkingDay.Tuesday |
                                                     WorkingDay.Wednesday | WorkingDay.Thursday |
                                                     WorkingDay.Friday | WorkingDay.Saturday;
    }

    /// <summary>
    /// Checks if the working days follow a standard Monday-Friday pattern.
    /// </summary>
    public static bool IsStandardWeekdays(this WorkingDay days)
    {
        return days == Patterns.Weekdays;
    }

    /// <summary>
    /// Checks if today is a working day based on the flags.
    /// </summary>
    public static bool IsTodayWorkingDay(this WorkingDay days)
    {
        return days.IncludesDay(DateTime.UtcNow.DayOfWeek);
    }

    /// <summary>
    /// Checks if a specific date is a working day based on the flags.
    /// </summary>
    public static bool IsWorkingDay(this WorkingDay days, DateTime date)
    {
        return days.IncludesDay(date.DayOfWeek);
    }

    /// <summary>
    /// Gets the next working day after a given date.
    /// </summary>
    public static DateTime GetNextWorkingDay(this WorkingDay days, DateTime fromDate)
    {
        if (days == WorkingDay.None)
        {
            throw new InvalidOperationException("No working days defined.");
        }

        var currentDate = fromDate.Date.AddDays(1);
        var maxAttempts = 7; // Search up to one week ahead
        var attempts = 0;

        while (attempts < maxAttempts)
        {
            if (days.IsWorkingDay(currentDate))
            {
                return currentDate;
            }

            currentDate = currentDate.AddDays(1);
            attempts++;
        }

        throw new InvalidOperationException("Could not find next working day within one week.");
    }
}
