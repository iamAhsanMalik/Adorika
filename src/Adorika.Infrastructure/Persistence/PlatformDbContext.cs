using Adorika.Application.Common.Interfaces.Persistence;
using Adorika.Domain.Entities;
using Adorika.Domain.Entities.Identity;
using Adorika.Domain.Entities.MultiTenancy;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Adorika.Infrastructure.Persistence;

/// <summary>
/// Platform database context for global system entities (NO tenant isolation).
/// This context is NOT multi-tenant aware and is used for:
/// - System installation
/// - Tenant management (stores ALL tenants)
/// - Platform users (SuperAdmins)
/// - System configuration
/// - Cross-tenant operations
/// - Migration command
/// dotnet ef migrations add initialPlatformSchema  -c PlatformDbContext -o Migrations/Platform -s src/Adorika.Api -p src/Adorika.Infrastructure
/// </summary>
public class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : IdentityDbContext<PlatformUser, PlatformRole, Guid>(options), IPlatformDbContext
{

    // ===== Platform Identity Tables =====
    public new DbSet<PlatformUser> Users => Set<PlatformUser>();
    public new DbSet<PlatformRole> Roles => Set<PlatformRole>();
    public DbSet<PlatformRolePermission> RolePermissions => Set<PlatformRolePermission>();

    // ===== Multi-tenancy (ALL tenants stored here) =====
    public DbSet<AppTenantInfo> Tenants => Set<AppTenantInfo>();

    // ===== System Configuration =====
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();

    // ===== Installation Audit =====
    public DbSet<InstallationAudit> InstallationAudits => Set<InstallationAudit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigurePlatformIdentity(builder);
        ConfigureTenants(builder);
        ConfigureSystemConfiguration(builder);
        ConfigureInstallationAudit(builder);
    }

    private void ConfigurePlatformIdentity(ModelBuilder builder)
    {
        // Configure PlatformUser with Platform-prefixed indexes
        builder.Entity<PlatformUser>(entity =>
        {
            entity.ToTable("PlatformUsers");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("PlatformEmailIndex");
            entity.HasIndex(e => e.NormalizedUserName).IsUnique().HasDatabaseName("PlatformUserNameIndex");
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.MfaSecret).HasMaxLength(500);
            entity.Property(e => e.LastLoginIp).HasMaxLength(50);
        });

        // Configure PlatformRole with Platform-prefixed indexes
        builder.Entity<PlatformRole>(entity =>
        {
            entity.ToTable("PlatformRoles");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.NormalizedName).IsUnique().HasDatabaseName("PlatformRoleNameIndex");
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.NormalizedName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            // Relationships
            entity.HasMany(r => r.Permissions)
                .WithOne(p => p.Role)
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure IdentityUserRole
        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("PlatformUserRoles");
        });

        // Configure IdentityUserClaim
        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("PlatformUserClaims");
        });

        // Configure IdentityUserLogin
        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("PlatformUserLogins");
        });

        // Configure IdentityRoleClaim
        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("PlatformRoleClaims");
        });

        // Configure IdentityUserToken
        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("PlatformUserTokens");
        });

        // Configure PlatformRolePermission
        builder.Entity<PlatformRolePermission>(entity =>
        {
            entity.ToTable("PlatformRolePermissions");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Resource).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Action).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Scope).HasMaxLength(50);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.HasIndex(p => new { p.RoleId, p.Resource, p.Action }).IsUnique();
        });
    }

    private void ConfigureTenants(ModelBuilder builder)
    {
        // Configure AppTenantInfo - stores ALL tenants (not tenant-scoped)
        builder.Entity<AppTenantInfo>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Identifier).IsUnique();
            entity.Property(t => t.Id).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Identifier).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.ContactEmail).HasMaxLength(256);
            entity.Property(t => t.SuspensionReason).HasMaxLength(500);
        });
    }

    private void ConfigureSystemConfiguration(ModelBuilder builder)
    {
        // Configure SystemConfiguration - system-wide singleton
        builder.Entity<SystemConfiguration>(entity =>
        {
            entity.ToTable("SystemConfiguration");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.InitialTenantId).HasMaxLength(100);
            entity.Property(s => s.SchemaVersion).HasMaxLength(100);
        });
    }

    private void ConfigureInstallationAudit(ModelBuilder builder)
    {
        // Configure InstallationAudit - tracks installation attempts
        builder.Entity<InstallationAudit>(entity =>
        {
            entity.ToTable("InstallationAudits");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(a => a.TenantIdentifier).HasMaxLength(100).IsRequired();
            entity.Property(a => a.SuperUserEmail).HasMaxLength(256).IsRequired();
            entity.Property(a => a.SchemaVersion).HasMaxLength(100).IsRequired();
            entity.Property(a => a.IpAddress).HasMaxLength(50);
            entity.Property(a => a.UserAgent).HasMaxLength(500);
            entity.Property(a => a.DatabaseHost).HasMaxLength(255);
            entity.Property(a => a.DatabaseName).HasMaxLength(63);
            entity.Property(a => a.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(a => a.InitiatedAt);
            entity.HasIndex(a => a.IsSuccessful);
        });
    }
}
