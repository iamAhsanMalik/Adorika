using Adorika.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Adorika.Application.Common.Persistence;

public interface ITenantDbContext
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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
