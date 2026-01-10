namespace Adorika.Domain.Enums;

/// <summary>
/// Represents the type of time-off for a user.
/// </summary>
public enum TimeOffType
{
    /// <summary>
    /// Annual vacation leave.
    /// </summary>
    Vacation = 0,

    /// <summary>
    /// Public or company holiday.
    /// </summary>
    Holiday = 1,

    /// <summary>
    /// Sick leave.
    /// </summary>
    SickLeave = 2,

    /// <summary>
    /// Personal leave for personal matters.
    /// </summary>
    PersonalLeave = 3,

    /// <summary>
    /// Maternity or paternity leave.
    /// </summary>
    ParentalLeave = 4,

    /// <summary>
    /// Bereavement leave.
    /// </summary>
    Bereavement = 5,

    /// <summary>
    /// Unpaid leave.
    /// </summary>
    UnpaidLeave = 6,

    /// <summary>
    /// Sabbatical leave.
    /// </summary>
    Sabbatical = 7,

    /// <summary>
    /// Other type of leave.
    /// </summary>
    Other = 99
}
