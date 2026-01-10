namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a permission assigned to a group.
/// Enables group-based permission management alongside role-based permissions.
/// </summary>
public class GroupPermission
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this permission belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== GROUP ASSOCIATION =====
    /// <summary>
    /// The group ID this permission is assigned to.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Navigation property to the group.
    /// </summary>
    public virtual Group Group { get; set; } = null!;

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
    /// Optional conditions in JSON format for ABAC rules.
    /// Example: {"department": "IT", "maxAmount": 10000}
    /// </summary>
    public string? Conditions { get; set; }

    // ===== METADATA =====
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority level for permission resolution (higher wins in conflicts).
    /// Default: 0
    /// </summary>
    public int Priority { get; set; } = 0;

    // ===== TRACKING =====
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    // ===== FACTORY METHODS =====
    public static GroupPermission Create(
        string tenantId,
        Guid groupId,
        string resource,
        string action,
        string? scope = null,
        string? conditions = null,
        string? description = null,
        int priority = 0,
        Guid? createdByUserId = null)
    {
        return new GroupPermission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            GroupId = groupId,
            Resource = resource,
            Action = action,
            Scope = scope,
            Conditions = conditions,
            Description = description,
            Priority = priority,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
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
    public void Update(
        string? resource = null,
        string? action = null,
        string? scope = null,
        string? conditions = null,
        string? description = null,
        int? priority = null,
        Guid? updatedByUserId = null)
    {
        if (!string.IsNullOrEmpty(resource))
        {
            Resource = resource;
        }

        if (!string.IsNullOrEmpty(action))
        {
            Action = action;
        }

        if (scope != null)
        {
            Scope = scope;
        }

        if (conditions != null)
        {
            Conditions = conditions;
        }

        if (description != null)
        {
            Description = description;
        }

        if (priority.HasValue)
        {
            Priority = priority.Value;
        }

        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    public void Deactivate(Guid? updatedByUserId = null)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    public void Activate(Guid? updatedByUserId = null)
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    /// <summary>
    /// Checks if this permission matches the given resource, action, and optional scope.
    /// </summary>
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

        // If no scope specified in query, any scope matches
        if (string.IsNullOrEmpty(scope))
        {
            return true;
        }

        // If scope specified, it must match or permission must have no scope (wildcard)
        return string.IsNullOrEmpty(Scope) ||
               Scope.Equals(scope, StringComparison.OrdinalIgnoreCase);
    }
}
