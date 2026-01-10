using Finbuckle.MultiTenant.Abstractions;

namespace Adorika.Domain.Entities.MultiTenancy;

public record AppTenantInfo(string Id, string Identifier, string Name) : TenantInfo(Id, Identifier, Name)
{
    public string? ConnectionString { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSuspended { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool CanAccess => IsActive && !IsSuspended;

    public static AppTenantInfo Create(string identifier, string name, string? contactEmail = null)
    {
        var normalized = identifier.ToLowerInvariant();
        var tenant = new AppTenantInfo(normalized, normalized, name)
        {
            ContactEmail = contactEmail,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        return tenant;
    }

    public AppTenantInfo Deactivate()
    {
        return this with { IsActive = false, UpdatedAt = DateTime.UtcNow };
    }

    public AppTenantInfo Suspend()
    {
        return this with { IsSuspended = true, UpdatedAt = DateTime.UtcNow };
    }

    public AppTenantInfo Reactivate()
    {
        return this with { IsActive = true, IsSuspended = false, UpdatedAt = DateTime.UtcNow };
    }
}
