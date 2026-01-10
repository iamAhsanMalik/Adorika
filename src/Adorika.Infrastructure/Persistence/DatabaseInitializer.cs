using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adorika.Infrastructure.Persistence;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task MigrateAsync(CancellationToken cancellationToken = default);
}

public class DatabaseInitializer(
    IServiceProvider serviceProvider,
    ILogger logger) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.Information("Starting database initialization...");

            // Apply migrations
            await MigrateAsync(cancellationToken);

            logger.Information("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        // Use scoped service provider to create a scope for the DbContext
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        logger.Information("Checking for pending migrations...");

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        var pendingMigrationsList = pendingMigrations.ToList();

        if (pendingMigrationsList.Count != 0)
        {
            logger.Information("Applying {Count} pending migrations: {Migrations}",
                pendingMigrationsList.Count,
                string.Join(", ", pendingMigrationsList));

            await context.Database.MigrateAsync(cancellationToken);

            logger.Information("Migrations applied successfully");
        }
        else
        {
            logger.Information("Database is up to date. No pending migrations");
        }
    }
}
