namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a security audit log entry for tracking security-related events.
/// Essential for compliance, forensics, and security monitoring.
/// </summary>
public class SecurityAuditLog
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this log entry belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== USER ASSOCIATION =====
    /// <summary>
    /// The user ID associated with this event (null for anonymous events).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The username at the time of the event (for historical tracking).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The email at the time of the event (for historical tracking).
    /// </summary>
    public string? Email { get; set; }

    // ===== EVENT INFORMATION =====
    /// <summary>
    /// The type of security event.
    /// Examples: "Login", "LoginFailed", "Logout", "PasswordChange", "MfaEnabled",
    /// "MfaDisabled", "AccountLocked", "PasswordReset", "SocialLogin", "SessionRevoked"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Category of the event for filtering and reporting.
    /// Examples: "Authentication", "Authorization", "Account", "MFA", "Session"
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the event.
    /// Examples: "Info", "Warning", "Error", "Critical"
    /// </summary>
    public string Severity { get; set; } = "Info";

    /// <summary>
    /// Detailed description of the event.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Additional event details in JSON format.
    /// Can include structured data like: {"reason": "Invalid password", "attempts": 3}
    /// </summary>
    public string? Details { get; set; }

    // ===== OUTCOME =====
    /// <summary>
    /// Indicates if the event/action was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if the event failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code if applicable.
    /// </summary>
    public string? ErrorCode { get; set; }

    // ===== REQUEST CONTEXT =====
    /// <summary>
    /// IP address from which the event originated.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User agent string from the request.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Device information if available.
    /// Example: "Chrome on Windows 11"
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Geographic location if available.
    /// Example: "New York, USA"
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// The endpoint or resource that was accessed.
    /// Example: "/api/auth/login", "/api/employees/123"
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// HTTP method if applicable.
    /// </summary>
    public string? HttpMethod { get; set; }

    // ===== SESSION TRACKING =====
    /// <summary>
    /// Session ID if the event occurred within a session.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Request ID for correlation with application logs.
    /// </summary>
    public string? RequestId { get; set; }

    // ===== RISK ASSESSMENT =====
    /// <summary>
    /// Risk score for the event (0-100).
    /// Higher scores indicate more suspicious activity.
    /// </summary>
    public int? RiskScore { get; set; }

    /// <summary>
    /// Risk factors that contributed to the risk score.
    /// Example: "New location, unusual time, multiple failed attempts"
    /// </summary>
    public string? RiskFactors { get; set; }

    // ===== TIMESTAMP =====
    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationUser? User { get; set; }

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Checks if the event is a failed authentication attempt.
    /// </summary>
    public bool IsFailedAuthentication() =>
        Category == "Authentication" && !IsSuccess;

    /// <summary>
    /// Checks if the event is high risk.
    /// </summary>
    public bool IsHighRisk() =>
        RiskScore.HasValue && RiskScore.Value >= 70;

    /// <summary>
    /// Checks if the event is critical.
    /// </summary>
    public bool IsCritical() =>
        Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Factory method to create a login event.
    /// </summary>
    public static SecurityAuditLog CreateLoginEvent(
        Guid userId,
        string tenantId,
        string username,
        string email,
        bool isSuccess,
        string ipAddress,
        string? userAgent = null,
        string? errorMessage = null)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Username = username,
            Email = email,
            EventType = isSuccess ? "Login" : "LoginFailed",
            Category = "Authentication",
            Severity = isSuccess ? "Info" : "Warning",
            Description = isSuccess ? "User logged in successfully" : "Login attempt failed",
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a logout event.
    /// </summary>
    public static SecurityAuditLog CreateLogoutEvent(
        Guid userId,
        string tenantId,
        string username,
        string ipAddress,
        string? sessionId = null)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Username = username,
            EventType = "Logout",
            Category = "Authentication",
            Severity = "Info",
            Description = "User logged out",
            IsSuccess = true,
            IpAddress = ipAddress,
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create an MFA event.
    /// </summary>
    public static SecurityAuditLog CreateMfaEvent(
        Guid userId,
        string tenantId,
        string username,
        string mfaAction,
        string mfaMethod,
        bool isSuccess,
        string ipAddress)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Username = username,
            EventType = mfaAction,
            Category = "MFA",
            Severity = isSuccess ? "Info" : "Warning",
            Description = $"MFA {mfaAction}: {mfaMethod}",
            IsSuccess = isSuccess,
            IpAddress = ipAddress,
            Details = $"{{\"method\": \"{mfaMethod}\"}}",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a password change event.
    /// </summary>
    public static SecurityAuditLog CreatePasswordChangeEvent(
        Guid userId,
        string tenantId,
        string username,
        bool isSuccess,
        string ipAddress,
        bool isReset = false)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Username = username,
            EventType = isReset ? "PasswordReset" : "PasswordChange",
            Category = "Account",
            Severity = "Info",
            Description = isReset ? "Password reset" : "Password changed",
            IsSuccess = isSuccess,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create an account locked event.
    /// </summary>
    public static SecurityAuditLog CreateAccountLockedEvent(
        Guid userId,
        string tenantId,
        string username,
        string reason,
        string ipAddress)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Username = username,
            EventType = "AccountLocked",
            Category = "Account",
            Severity = "Warning",
            Description = $"Account locked: {reason}",
            IsSuccess = true,
            IpAddress = ipAddress,
            Details = $"{{\"reason\": \"{reason}\"}}",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a session revoked event.
    /// </summary>
    public static SecurityAuditLog CreateSessionRevokedEvent(
        Guid userId,
        string tenantId,
        string username,
        string sessionId,
        string reason,
        string ipAddress)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Username = username,
            EventType = "SessionRevoked",
            Category = "Session",
            Severity = "Info",
            Description = $"Session revoked: {reason}",
            IsSuccess = true,
            IpAddress = ipAddress,
            SessionId = sessionId,
            Details = $"{{\"reason\": \"{reason}\"}}",
            Timestamp = DateTime.UtcNow
        };
    }
}
