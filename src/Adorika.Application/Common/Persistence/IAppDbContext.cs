using Adorika.Domain.Entities;
using Adorika.Domain.Entities.Identity;
using Adorika.Domain.Entities.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Adorika.Application.Common.Persistence;

public interface IAppDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<IdentityUserRole<Guid>> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<Group> Groups { get; }
    DbSet<UserGroup> UserGroups { get; }
    DbSet<GroupPermission> GroupPermissions { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<MfaMethod> MfaMethods { get; }
    DbSet<SocialLogin> SocialLogins { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<SecurityAuditLog> SecurityAuditLogs { get; }
    DbSet<SystemConfiguration> SystemConfigurations { get; }
    DbSet<AppTenantInfo> Tenants { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
