using Adorika.Application.Common.Interfaces.Persistence;
using Adorika.Application.Common.Interfaces.Services;
using Adorika.Application.Common.Models;
using Adorika.Application.Common.Wrapper;
using Adorika.Application.Features.Installation.GetInstallationStatus;
using Adorika.Application.Features.Installation.InstallSystem;
using Adorika.Domain.Constants;
using Adorika.Domain.Entities;
using Adorika.Domain.Entities.Identity;
using Adorika.Domain.Entities.MultiTenancy;
using Adorika.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    public async Task<Result<InstallSystemResponse>> InstallSystem(
        InstallSystemCommand request,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        _logger.Information("Starting System Installation for Tenant: {Tenant}", request.TenantIdentifier);

        var dbDto = new DbConnectionDto(request.DatabaseHost, request.DatabasePort, request.DatabaseName, request.DatabaseUsername, request.DatabasePassword);
        var connectionString = _databaseUtility.BuildConnectionString(dbDto);

        IDbContextTransaction? transaction = null;
        InstallationAudit? auditRecord = null;

        try
        {
            // 1. Database Infrastructure Setup
            await _databaseUtility.CreateDatabaseIfNotExists(dbDto, ct);

            // 2. Schema Migrations (Both Contexts)
            await _databaseUtility.Migrate(isPlatform: true, ct);
            await _databaseUtility.Migrate(isPlatform: false, ct);

            // 3. Create fresh context for transaction
            var options = new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(connectionString).Options;
            await using var context = new PlatformDbContext(options);

            // 4. Create audit record
            auditRecord = InstallationAudit.CreateStarted(
                request.TenantIdentifier,
                ipAddress,
                userAgent,
                request.DatabaseHost,
                request.DatabaseName);
            context.InstallationAudits.Add(auditRecord);
            await context.SaveChangesAsync(ct);

            // 5. Begin Transaction for atomicity
            transaction = await context.Database.BeginTransactionAsync(ct);
            _logger.Information("Transaction started for system installation");

            // 6. Bootstrap Data
            (string tenantId, Guid superAdminId, string tenantIdentifier, string superUserEmail) = await BootstrapPlatformData(context, request, ct);

            // 7. Finalize and raise domain events
            await MarkSystemInitializedAsync(context, tenantId, superAdminId, ipAddress, userAgent, ct);

            // 8. Update audit record to mark as completed
            if (auditRecord != null)
            {
                auditRecord.MarkAsCompleted(tenantId, superAdminId, superUserEmail, SchemaVersions.Current);
            }

            // 9. Commit transaction
            await transaction.CommitAsync(ct);
            _logger.Information("System installation completed successfully for Tenant: {Tenant}", request.TenantIdentifier);

            return Result<InstallSystemResponse>.Success(new InstallSystemResponse
            {
                TenantId = tenantId,
                TenantIdentifier = tenantIdentifier,
                SuperUserId = superAdminId,
                SuperUserEmail = superUserEmail,
                SchemaVersion = SchemaVersions.Current,
                DatabaseConfigured = true
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Installation fatal error for Tenant: {Tenant}", request.TenantIdentifier);

            // Mark audit record as failed
            if (auditRecord != null)
            {
                try
                {
                    var options = new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(connectionString).Options;
                    await using var auditContext = new PlatformDbContext(options);
                    var auditToUpdate = await auditContext.InstallationAudits.FindAsync(new object[] { auditRecord.Id }, ct);
                    if (auditToUpdate != null)
                    {
                        auditToUpdate.MarkAsFailed(ex.Message);
                        await auditContext.SaveChangesAsync(ct);
                    }
                }
                catch (Exception auditEx)
                {
                    _logger.Error(auditEx, "Failed to update audit record for failed installation");
                }
            }

            // Rollback transaction if it exists
            if (transaction != null)
            {
                try
                {
                    await transaction.RollbackAsync(ct);
                    _logger.Information("Transaction rolled back due to error");
                }
                catch (Exception rollbackEx)
                {
                    _logger.Error(rollbackEx, "Failed to rollback transaction");
                }
            }

            return Result<InstallSystemResponse>.Failure(ResultError.InternalError("System", ex.Message));
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
    public async Task<bool> TestDatabaseConnection(DbConnectionDto db, CancellationToken ct)
    {
        return await databaseUtility.CanConnect(db, ct);
    }

    private async Task<(string tenantId, Guid superAdminId, string tenantIdentifier, string superUserEmail)> BootstrapPlatformData(
        PlatformDbContext context,
        InstallSystemCommand req,
        CancellationToken ct)
    {
        _logger.Information("Bootstrapping platform data: Creating tenant and super user");

        // Create tenant
        var tenant = AppTenantInfo.Create(req.TenantIdentifier.ToLower(), req.TenantName, req.TenantContactEmail);
        context.Tenants.Add(tenant);

        // Create super admin user
        var superAdmin = PlatformUser.CreateSuperAdmin(req.SuperUserEmail, req.SuperUserEmail, req.SuperUserFirstName, req.SuperUserLastName);
        superAdmin.PasswordHash = _passwordHasher.HashPassword(superAdmin, req.SuperUserPassword);
        context.Users.Add(superAdmin);

        await context.SaveChangesAsync(ct);
        _logger.Information("Platform data bootstrapped: Tenant={TenantId}, SuperUser={SuperUserId}", tenant.Id, superAdmin.Id);

        return (tenant.Id, superAdmin.Id, tenant.Identifier, superAdmin.Email ?? req.SuperUserEmail);
    }

    private async Task MarkSystemInitializedAsync(
        PlatformDbContext context,
        string tenantId,
        Guid adminId,
        string? ipAddress,
        string? userAgent,
        CancellationToken ct)
    {
        _logger.Information("Marking system as initialized");

        var config = new SystemConfiguration();
        config.MarkAsInitialized(tenantId, adminId, SchemaVersions.Current, ipAddress, userAgent);
        context.SystemConfigurations.Add(config);

        await context.SaveChangesAsync(ct);

        // Domain events will be dispatched here if you have a domain event dispatcher
        // For now, we log the event for audit purposes
        foreach (var domainEvent in config.DomainEvents)
        {
            _logger.Information("Domain Event Raised: {EventType} at {OccurredAt}",
                domainEvent.GetType().Name,
                domainEvent.OccurredAt);
        }

        config.ClearDomainEvents();
        _logger.Information("System marked as initialized with schema version: {SchemaVersion}", SchemaVersions.Current);
    }
}
