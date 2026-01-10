using Adorika.Application.Common.Persistence;
using Adorika.Domain.Entities;
using Adorika.Domain.Entities.Identity;
using Adorika.Domain.Entities.MultiTenancy;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Adorika.Infrastructure.Persistence;
/// <summary>
/// To generate migrations for RBAC schema, use the following command from the solution root:
/// dotnet ef migrations add initialMigrationForRbac -s src/Adorika.Api -p src/Adorika.Infrastructure -o Persistence/Migrations
/// </summary>
/// <param name="multiTenantContextAccessor"></param>
/// <param name="options"></param>
public class AppDbContext(IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor, DbContextOptions<AppDbContext> options) : MultiTenantIdentityDbContext<ApplicationUser, ApplicationRole, Guid>(multiTenantContextAccessor, options), IAppDbContext
{
    // Users
    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    // Groups, Roles and Permissions
    public new DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
    public new DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();

    // Sessions and Security
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MfaMethod> MfaMethods => Set<MfaMethod>();
    public DbSet<SocialLogin> SocialLogins => Set<SocialLogin>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    // Audit Logs
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();

    // Multi-tenancy
    public DbSet<AppTenantInfo> Tenants => Set<AppTenantInfo>();

    // System Configuration
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigureEntities(builder);
    }
    public static void ConfigureEntities(ModelBuilder builder)
    {
        // Configure AppTenantInfo (NOT multi-tenant - it stores all tenants)
        builder.Entity<AppTenantInfo>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Identifier).IsUnique();
            entity.Property(t => t.Id).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Identifier).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.ContactEmail).HasMaxLength(256);

            // Explicitly mark as NOT multi-tenant - Tenants table contains all tenants
            entity.IsNotMultiTenant();
        });

        // Configure SystemConfiguration (NOT multi-tenant - system-wide singleton)
        builder.Entity<SystemConfiguration>(entity =>
        {
            entity.ToTable("SystemConfiguration");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.InitialTenantId).HasMaxLength(100);
            entity.Property(s => s.SchemaVersion).HasMaxLength(100);

            // Explicitly mark as NOT multi-tenant - this is a system-wide configuration entity
            entity.IsNotMultiTenant();
        });

        // Configure Identity tables with custom names
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.UserName }).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            // TenantId is nullable for platform-level users (SystemAdmin)
            entity.Property(e => e.TenantId).HasMaxLength(100).IsRequired(false);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            // TenantId is nullable for platform-level roles (SystemAdmin)
            entity.Property(e => e.TenantId).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
            // TenantId is nullable for platform-level role assignments (SystemAdmin)
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired(false);
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
            // TenantId is nullable for platform-level claims
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired(false);
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
            // TenantId is nullable for platform-level logins
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired(false);
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
            // TenantId is nullable for platform-level role claims
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired(false);
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
            // TenantId is nullable for platform-level tokens
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired(false);
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(p => p.Id);
            // TenantId is nullable for platform-level permissions (SystemAdmin)
            entity.Property(p => p.TenantId).HasMaxLength(100).IsRequired(false);
            entity.Property(p => p.Resource).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Action).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Scope).HasMaxLength(50);
            entity.HasIndex(p => new { p.TenantId, p.RoleId, p.Resource, p.Action }).IsUnique();
        });

        builder.Entity<UserSession>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(s => s.SessionToken).HasMaxLength(500).IsRequired();
            entity.Property(s => s.SessionId).HasMaxLength(100).IsRequired();
            entity.Property(s => s.DeviceInfo).HasMaxLength(500);
            entity.Property(s => s.IpAddress).HasMaxLength(50);
            entity.HasIndex(s => new { s.TenantId, s.UserId, s.IsActive });
            entity.HasIndex(s => s.SessionToken);
            entity.IsMultiTenant();
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(r => r.TokenHash).HasMaxLength(500).IsRequired();
            entity.Property(r => r.DeviceId).HasMaxLength(100);
            entity.HasIndex(r => new { r.TenantId, r.UserId, r.IsRevoked });
            entity.HasIndex(r => r.TokenHash);
            entity.IsMultiTenant();
        });

        builder.Entity<MfaMethod>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(m => m.MethodType).HasMaxLength(50).IsRequired();
            entity.Property(m => m.PhoneNumber).HasMaxLength(50);
            entity.Property(m => m.Email).HasMaxLength(256);
            entity.HasIndex(m => new { m.TenantId, m.UserId, m.IsEnabled });
            entity.IsMultiTenant();
        });

        builder.Entity<Group>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(g => g.Name).HasMaxLength(200).IsRequired();
            entity.Property(g => g.NormalizedName).HasMaxLength(200).IsRequired();
            entity.Property(g => g.Description).HasMaxLength(1000);
            entity.HasIndex(g => new { g.TenantId, g.NormalizedName }).IsUnique();
            entity.HasIndex(g => new { g.TenantId, g.IsActive });

            // Self-referencing relationship for hierarchical groups
            entity.HasOne(g => g.ParentGroup)
                .WithMany(g => g.ChildGroups)
                .HasForeignKey(g => g.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(g => g.Permissions)
                .WithOne(gp => gp.Group)
                .HasForeignKey(gp => gp.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(g => g.UserGroups)
                .WithOne(ug => ug.Group)
                .HasForeignKey(ug => ug.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.IsMultiTenant();
        });

        builder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(ug => ug.Id);
            entity.Property(ug => ug.TenantId).HasMaxLength(100).IsRequired();
            entity.HasIndex(ug => new { ug.TenantId, ug.UserId, ug.GroupId }).IsUnique();
            entity.HasIndex(ug => new { ug.TenantId, ug.UserId, ug.IsActive });
            entity.HasIndex(ug => new { ug.TenantId, ug.GroupId, ug.IsActive });
            entity.IsMultiTenant();
        });

        builder.Entity<GroupPermission>(entity =>
        {
            entity.HasKey(gp => gp.Id);
            entity.Property(gp => gp.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(gp => gp.Resource).HasMaxLength(100).IsRequired();
            entity.Property(gp => gp.Action).HasMaxLength(50).IsRequired();
            entity.Property(gp => gp.Scope).HasMaxLength(50);
            entity.Property(gp => gp.Conditions).HasMaxLength(2000);
            entity.Property(gp => gp.Description).HasMaxLength(500);
            entity.HasIndex(gp => new { gp.TenantId, gp.GroupId, gp.Resource, gp.Action });
            entity.HasIndex(gp => new { gp.TenantId, gp.IsActive });
            entity.IsMultiTenant();
        });

        builder.Entity<SocialLogin>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Provider).HasMaxLength(50).IsRequired();
            entity.Property(s => s.ProviderKey).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Email).HasMaxLength(256);
            entity.HasIndex(s => new { s.Provider, s.ProviderKey }).IsUnique();
            entity.HasIndex(s => new { s.TenantId, s.UserId });
            entity.IsMultiTenant();
        });

        builder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(p => p.TokenHash).HasMaxLength(500).IsRequired();
            entity.Property(p => p.TokenIdentifier).HasMaxLength(100);
            entity.Property(p => p.SentToEmail).HasMaxLength(256).IsRequired();
            entity.Property(p => p.RequestedFromIp).HasMaxLength(50).IsRequired();
            entity.HasIndex(p => p.TokenHash);
            entity.HasIndex(p => new { p.TenantId, p.UserId, p.IsUsed });
            entity.IsMultiTenant();
        });

        builder.Entity<SecurityAuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EventType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Category).HasMaxLength(50).IsRequired();
            entity.Property(a => a.Severity).HasMaxLength(20).IsRequired();
            entity.Property(a => a.IpAddress).HasMaxLength(50).IsRequired();
            entity.HasIndex(a => new { a.TenantId, a.Timestamp });
            entity.HasIndex(a => new { a.TenantId, a.UserId, a.EventType });
            entity.IsMultiTenant();
        });

        // Configure relationships
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.MfaMethods)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.SocialLogins)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.UserGroups)
            .WithOne(ug => ug.User)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationRole>()
            .HasMany(r => r.Permissions)
            .WithOne(p => p.Role)
            .HasForeignKey(p => p.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
