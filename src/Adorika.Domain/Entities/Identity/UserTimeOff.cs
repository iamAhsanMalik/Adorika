using Adorika.Domain.Entities.Base;

namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a time-off period (holiday, vacation, sick leave) for a user.
/// Users cannot login during their time-off periods.
/// </summary>
public class UserTimeOff : FullAuditedEntity
{

    // ===== USER ASSOCIATION =====
    public Guid UserId { get; set; }

    // ===== TIME-OFF DETAILS =====
    /// <summary>
    /// The type of time-off (e.g., Vacation, Holiday, SickLeave, PersonalLeave).
    /// </summary>
    public string TimeOffType { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the time-off period (inclusive).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the time-off period (inclusive).
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Optional description or reason for the time-off.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if this time-off is approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// User ID of the approver (manager, admin, etc.).
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// When the time-off was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Indicates if this time-off is active (not cancelled).
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationUser User { get; set; } = null!;

    // ===== COMPUTED PROPERTIES =====
    /// <summary>
    /// Checks if the time-off is currently active (today falls within the period).
    /// </summary>
    public bool IsCurrentlyActive
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            return IsActive && IsApproved && today >= StartDate.Date && today <= EndDate.Date;
        }
    }

    /// <summary>
    /// Checks if a specific date falls within this time-off period.
    /// </summary>
    public bool CoversDate(DateTime date)
    {
        var dateOnly = date.Date;
        return IsActive && IsApproved && dateOnly >= StartDate.Date && dateOnly <= EndDate.Date;
    }

    /// <summary>
    /// Gets the total number of days in this time-off period.
    /// </summary>
    public int TotalDays => (EndDate.Date - StartDate.Date).Days + 1;

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Approves the time-off request.
    /// </summary>
    public void Approve(Guid approvedBy)
    {
        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Cancels the time-off.
    /// </summary>
    public void Cancel()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Reactivates a cancelled time-off.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the time-off period.
    /// </summary>
    public void UpdatePeriod(DateTime startDate, DateTime endDate, string? description = null)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.");
        }

        StartDate = startDate;
        EndDate = endDate;
        if (description != null)
        {
            Description = description;
        }
        MarkAsUpdated();
    }

    // ===== FACTORY METHODS =====
    /// <summary>
    /// Creates a new time-off request.
    /// </summary>
    public static UserTimeOff Create(
        Guid userId,
        string tenantId,
        string timeOffType,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        bool requiresApproval = true)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.");
        }

        return new UserTimeOff
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            TimeOffType = timeOffType,
            StartDate = startDate,
            EndDate = endDate,
            Description = description,
            IsApproved = !requiresApproval, // Auto-approve if no approval required
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a pre-approved time-off (e.g., company holidays).
    /// </summary>
    public static UserTimeOff CreatePreApproved(
        Guid userId,
        string tenantId,
        string timeOffType,
        DateTime startDate,
        DateTime endDate,
        string? description = null)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.");
        }

        return new UserTimeOff
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            TimeOffType = timeOffType,
            StartDate = startDate,
            EndDate = endDate,
            Description = description,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
