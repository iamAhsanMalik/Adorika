using Adorika.Application.Common.Interfaces.Persistence;
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
/// - Migration command
/// dotnet ef migrations add initialTenantSchema  -c TenantDbContext -o Migrations/Tenant -s src/Adorika.Api -p src/Adorika.Infrastructure
/// </summary>
/// <param name="multiTenantContextAccessor"></param>
/// <param name="options"></param>
public class TenantDbContext(IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor, DbContextOptions<TenantDbContext> options) : MultiTenantIdentityDbContext<ApplicationUser, ApplicationRole, Guid>(multiTenantContextAccessor, options), ITenantDbContext
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigureTenantIdentity(builder);
        ConfigureTenantEntities(builder);
    }
    private static void ConfigureTenantIdentity(ModelBuilder builder)
    {
        // Configure ApplicationUser (tenant-scoped) with Tenant-prefixed indexes
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.UserName }).IsUnique();
            entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("TenantEmailIndex");
            entity.HasIndex(e => e.NormalizedUserName).IsUnique().HasDatabaseName("TenantUserNameIndex");
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastLoginIp).HasMaxLength(50);

            // Relationships
            entity.HasMany(u => u.Sessions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.MfaMethods)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SocialLogins)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.RefreshTokens)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.UserGroups)
                .WithOne(ug => ug.User)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mark as multi-tenant
            entity.IsMultiTenant();
        });

        // Configure ApplicationRole (tenant-scoped) with Tenant-prefixed indexes
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => e.NormalizedName).IsUnique().HasDatabaseName("TenantRoleNameIndex");
            entity.Property(e => e.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            // Relationships
            entity.HasMany(r => r.Permissions)
                .WithOne(p => p.Role)
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mark as multi-tenant
            entity.IsMultiTenant();
        });

        // Configure Identity tables with TenantId and mark as multi-tenant
        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired();
            entity.IsMultiTenant();
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired();
            entity.IsMultiTenant();
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired();
            entity.IsMultiTenant();
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired();
            entity.IsMultiTenant();
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
            entity.Property<string>("TenantId").HasMaxLength(100).IsRequired();
            entity.IsMultiTenant();
        });

        // Configure RolePermission (tenant-scoped)
        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Resource).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Action).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Scope).HasMaxLength(50);
            entity.HasIndex(p => new { p.TenantId, p.RoleId, p.Resource, p.Action }).IsUnique();
            entity.IsMultiTenant();
        });
    }

    private static void ConfigureTenantEntities(ModelBuilder builder)
    {
        // Configure UserSession
        builder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");
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

        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(r => r.TokenHash).HasMaxLength(500).IsRequired();
            entity.Property(r => r.DeviceId).HasMaxLength(100);
            entity.HasIndex(r => new { r.TenantId, r.UserId, r.IsRevoked });
            entity.HasIndex(r => r.TokenHash);
            entity.IsMultiTenant();
        });

        // Configure MfaMethod
        builder.Entity<MfaMethod>(entity =>
        {
            entity.ToTable("MfaMethods");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(m => m.MethodType).HasMaxLength(50).IsRequired();
            entity.Property(m => m.PhoneNumber).HasMaxLength(50);
            entity.Property(m => m.Email).HasMaxLength(256);
            entity.HasIndex(m => new { m.TenantId, m.UserId, m.IsEnabled });
            entity.IsMultiTenant();
        });

        // Configure Group
        builder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(g => g.Name).HasMaxLength(200).IsRequired();
            entity.Property(g => g.NormalizedName).HasMaxLength(200).IsRequired();
            entity.Property(g => g.Description).HasMaxLength(1000);
            entity.HasIndex(g => new { g.TenantId, g.NormalizedName }).IsUnique();
            entity.HasIndex(g => new { g.TenantId, g.IsActive });

            // Self-referencing relationship
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

        // Configure UserGroup
        builder.Entity<UserGroup>(entity =>
        {
            entity.ToTable("UserGroups");
            entity.HasKey(ug => ug.Id);
            entity.Property(ug => ug.TenantId).HasMaxLength(100).IsRequired();
            entity.HasIndex(ug => new { ug.TenantId, ug.UserId, ug.GroupId }).IsUnique();
            entity.HasIndex(ug => new { ug.TenantId, ug.UserId, ug.IsActive });
            entity.HasIndex(ug => new { ug.TenantId, ug.GroupId, ug.IsActive });
            entity.IsMultiTenant();
        });

        // Configure GroupPermission
        builder.Entity<GroupPermission>(entity =>
        {
            entity.ToTable("GroupPermissions");
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

        // Configure SocialLogin
        builder.Entity<SocialLogin>(entity =>
        {
            entity.ToTable("SocialLogins");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.TenantId).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Provider).HasMaxLength(50).IsRequired();
            entity.Property(s => s.ProviderKey).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Email).HasMaxLength(256);
            entity.HasIndex(s => new { s.Provider, s.ProviderKey }).IsUnique();
            entity.HasIndex(s => new { s.TenantId, s.UserId });
            entity.IsMultiTenant();
        });

        // Configure PasswordResetToken
        builder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
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

        // Configure SecurityAuditLog
        builder.Entity<SecurityAuditLog>(entity =>
        {
            entity.ToTable("SecurityAuditLogs");
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
    }
}
