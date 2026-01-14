using Adorika.Application.Common.Interfaces.Persistence;
using Adorika.Application.Common.Models;
using Adorika.Application.Common.Wrapper;
using Adorika.Application.Features.Installation.GetInstallationStatus;
using Adorika.Application.Features.Installation.InstallSystem;
using Adorika.Domain.Entities;
using Adorika.Domain.Entities.Identity;
using Adorika.Domain.Entities.MultiTenancy;
using Adorika.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace Adorika.Infrastructure.Services;

/// <summary>
/// Handles one-time system installation process.
/// This service orchestrates database creation, migrations, and initial data seeding.
/// </summary>
public class SystemInstallation(
    IPlatformDbContext platformDbContext,
    IDatabaseUtility databaseUtility,
    IPasswordHasher<PlatformUser> passwordHasher,
    ILogger logger) : ISystemInstallation
{
    private readonly IPlatformDbContext _platformDbContext = platformDbContext;
    private readonly IDatabaseUtility _databaseUtility = databaseUtility;
    private readonly IPasswordHasher<PlatformUser> _passwordHasher = passwordHasher;
    private readonly ILogger _logger = logger;

    public async Task<Result<InstallationStatusResponse>> GetInstallationStatus(CancellationToken ct = default)
    {
        try
        {
            // 1. Check if the schema even exists
            var tableExists = await _databaseUtility.TableExists<SystemConfiguration>(isPlatform: true, ct);
            if (!tableExists)
            {
                return Result<InstallationStatusResponse>.Success(InstallationStatusResponse.NotInitialized())
                    .WithMessage("System database is not initialized.");
            }

            // 2. Check initialized flag
            var config = await _platformDbContext.SystemConfigurations.AsNoTracking().FirstOrDefaultAsync(ct);
            if (config is not { IsInitialized: true })
            {
                return Result<InstallationStatusResponse>.Success(InstallationStatusResponse.NotInitialized());
            }

            return Result<InstallationStatusResponse>.Success(
                InstallationStatusResponse.AlreadyInitialized(config.InitializedAt ?? DateTime.UtcNow, config.SchemaVersion))
                .WithMessage("System is already initialized.");
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to determine installation status.");
            return Result<InstallationStatusResponse>.Failure(ResultError.InternalError("System", ex.Message))
                .WithData(InstallationStatusResponse.NotInitialized());
        }
    }

    public async Task<Result<InstallSystemResponse>> InstallSystem(InstallSystemCommand request, CancellationToken ct = default)
    {
        _logger.Information("Starting System Installation for Tenant: {Tenant}", request.TenantIdentifier);

        var dbDto = new DbConnectionDto(request.DatabaseHost, request.DatabasePort, request.DatabaseName, request.DatabaseUsername, request.DatabasePassword);
        var connectionString = _databaseUtility.BuildConnectionString(dbDto);

        try
        {
            // 1. Database Infrastructure Setup
            await _databaseUtility.CreateDatabaseIfNotExists(dbDto, ct);

            // 2. Schema Migrations (Both Contexts)
            await _databaseUtility.Migrate(isPlatform: true, ct);
            await _databaseUtility.Migrate(isPlatform: false, ct);

            // 3. Bootstrap Data (Using a dynamic context factory or direct injection)
            // Note: Since the DB just changed, we often use a fresh context here
            (string tenantId, Guid superAdminId) = await BootstrapPlatformData(connectionString, request, ct);

            // 4. Finalize
            var schemaVersion = await MarkSystemInitializedAsync(connectionString, tenantId, superAdminId, ct);

            return Result<InstallSystemResponse>.Success(new InstallSystemResponse
            {
                TenantId = tenantId,
                SuperUserId = superAdminId,
                SchemaVersion = schemaVersion,
                // Provide the string for Docker Env persistence
                EnvironmentVariable = $"ConnectionStrings__DefaultConnection=\"{connectionString}\""
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Installation fatal error");
            return Result<InstallSystemResponse>.Failure(ResultError.InternalError("System", ex.Message));
        }
    }
    public async Task<bool> TestDatabaseConnection(DbConnectionDto db, CancellationToken ct)
    {
        return await databaseUtility.CanConnect(db, ct);
    }

    private async Task<(string tenantId, Guid superAdminId)> BootstrapPlatformData(string conn, InstallSystemCommand req, CancellationToken ct)
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(conn).Options;
        using var context = new PlatformDbContext(options);

        // Logic for Tenant, Roles, and User...
        var tenant = AppTenantInfo.Create(req.TenantIdentifier.ToLower(), req.TenantName, req.TenantContactEmail);
        context.Tenants.Add(tenant);

        var superAdmin = PlatformUser.CreateSuperAdmin(req.SuperUserEmail, req.SuperUserEmail, req.SuperUserFirstName, req.SuperUserLastName);
        superAdmin.PasswordHash = _passwordHasher.HashPassword(superAdmin, req.SuperUserPassword);
        context.Users.Add(superAdmin);

        await context.SaveChangesAsync(ct);
        return (tenant.Id, superAdmin.Id);
    }

    private async Task<string> MarkSystemInitializedAsync(string conn, string tenantId, Guid adminId, CancellationToken ct)
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(conn).Options;
        using var context = new PlatformDbContext(options);

        var config = new SystemConfiguration();
        config.MarkAsInitialized(tenantId, adminId, "1.0.0");
        context.SystemConfigurations.Add(config);

        await context.SaveChangesAsync(ct);
        return "1.0.0";
    }
}
