namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a permission assigned to a role.
/// Enables fine-grained permission-based authorization within a tenant.
/// </summary>
public class RolePermission
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this permission belongs to.
    /// This is REQUIRED - RolePermission is for tenant-scoped permissions only.
    /// For platform-level permissions (SystemAdmin), use PlatformRolePermission instead.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== ROLE ASSOCIATION =====
    public Guid RoleId { get; set; }

    // ===== PERMISSION DEFINITION =====
    /// <summary>
    /// The resource this permission applies to.
    /// Examples: "Employees", "Payroll", "Reports", "Settings"
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// The action allowed on the resource.
    /// Examples: "Read", "Write", "Delete", "Approve", "Export"
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Optional scope for the permission.
    /// Examples: "Own" (own data only), "Team", "Department", "All"
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Optional conditions in JSON format for complex permission rules.
    /// Example: {"department": "IT", "maxAmount": 10000}
    /// </summary>
    public string? Conditions { get; set; }

    // ===== METADATA =====
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // ===== TRACKING =====
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationRole Role { get; set; } = null!;

    // ===== FACTORY METHODS =====
    public static RolePermission Create(
        string tenantId,
        Guid roleId,
        string resource,
        string action,
        string? scope = null,
        string? description = null)
    {
        return string.IsNullOrWhiteSpace(tenantId)
            ? throw new ArgumentException("TenantId is required for RolePermission. Use PlatformRolePermission for platform-level permissions.", nameof(tenantId))
            : new RolePermission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleId = roleId,
            Resource = resource,
            Action = action,
            Scope = scope,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ===== COMPUTED PROPERTIES =====
    /// <summary>
    /// Returns the full permission string in format: Resource.Action
    /// Example: "Employees.Read", "Payroll.Approve"
    /// </summary>
    public string FullPermission => $"{Resource}.{Action}";

    /// <summary>
    /// Returns the full permission string with scope if available.
    /// Example: "Employees.Read.Own", "Payroll.Approve.All"
    /// </summary>
    public string FullPermissionWithScope =>
        string.IsNullOrEmpty(Scope) ? FullPermission : $"{Resource}.{Action}.{Scope}";

    // ===== DOMAIN METHODS =====
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool Matches(string resource, string action, string? scope = null)
    {
        if (!IsActive)
        {
            return false;
        }

        var resourceMatch = Resource.Equals(resource, StringComparison.OrdinalIgnoreCase);
        var actionMatch = Action.Equals(action, StringComparison.OrdinalIgnoreCase);

        if (!resourceMatch || !actionMatch)
        {
            return false;
        }

        // If no scope specified, any scope matches
        if (string.IsNullOrEmpty(scope))
        {
            return true;
        }

        // If scope specified, it must match
        return Scope?.Equals(scope, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
