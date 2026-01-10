using Adorika.Domain.Entities;
using Adorika.Domain.Entities.Identity;
using Adorika.Domain.Entities.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Adorika.Application.Common.Persistence;

/// <summary>
/// Interface for platform-level database operations (NO tenant isolation).
/// Used for:
/// - System installation
/// - Tenant management (ALL tenants)
/// - Platform users (SuperAdmins)
/// - System configuration
/// - Cross-tenant operations
/// </summary>
public interface IPlatformDbContext
{
    // ===== Platform Identity =====
    DbSet<PlatformUser> Users { get; }
    DbSet<PlatformRole> Roles { get; }
    DbSet<IdentityUserRole<Guid>> UserRoles { get; }
    DbSet<PlatformRolePermission> RolePermissions { get; }

    // ===== Multi-tenancy (ALL tenants) =====
    DbSet<AppTenantInfo> Tenants { get; }

    // ===== System Configuration =====
    DbSet<SystemConfiguration> SystemConfigurations { get; }

    // ===== Save Operations =====
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
