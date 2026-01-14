using Adorika.Domain.Entities.Base;

namespace Adorika.Domain.Entities;

/// <summary>
/// Audit record for system installation attempts and completions.
/// Provides complete audit trail for security and compliance.
/// </summary>
public class InstallationAudit : BaseEntity
{
    /// <summary>
    /// The ID of the tenant created during installation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The tenant identifier (slug).
    /// </summary>
    public string TenantIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the super user created during installation.
    /// </summary>
    public Guid SuperUserId { get; set; }

    /// <summary>
    /// Email of the super user created.
    /// </summary>
    public string SuperUserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Database schema version installed.
    /// </summary>
    public string SchemaVersion { get; set; } = string.Empty;

    /// <summary>
    /// IP address from which the installation was initiated.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the client that initiated installation.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Database host configured.
    /// </summary>
    public string? DatabaseHost { get; set; }

    /// <summary>
    /// Database name configured.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Timestamp when installation was initiated.
    /// </summary>
    public DateTime InitiatedAt { get; set; }

    /// <summary>
    /// Timestamp when installation completed successfully.
    /// Null if installation failed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Whether the installation completed successfully.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if installation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of the installation process in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Creates a new installation audit record for a started installation.
    /// </summary>
    public static InstallationAudit CreateStarted(
        string tenantIdentifier,
        string? ipAddress,
        string? userAgent,
        string? databaseHost,
        string? databaseName)
    {
        return new InstallationAudit
        {
            Id = Guid.NewGuid(),
            TenantIdentifier = tenantIdentifier,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DatabaseHost = databaseHost,
            DatabaseName = databaseName,
            InitiatedAt = DateTime.UtcNow,
            IsSuccessful = false
        };
    }

    /// <summary>
    /// Marks the installation as successfully completed.
    /// </summary>
    public void MarkAsCompleted(string tenantId, Guid superUserId, string superUserEmail, string schemaVersion)
    {
        TenantId = tenantId;
        SuperUserId = superUserId;
        SuperUserEmail = superUserEmail;
        SchemaVersion = schemaVersion;
        CompletedAt = DateTime.UtcNow;
        IsSuccessful = true;
        DurationMs = (long)(CompletedAt.Value - InitiatedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Marks the installation as failed.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        CompletedAt = DateTime.UtcNow;
        IsSuccessful = false;
        ErrorMessage = errorMessage;
        DurationMs = (long)(CompletedAt.Value - InitiatedAt).TotalMilliseconds;
    }
}
