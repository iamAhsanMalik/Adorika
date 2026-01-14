using Adorika.Application.Common.Models;
using Adorika.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Serilog;

namespace Adorika.Infrastructure.Services;

public interface IDatabaseUtility
{
    string BuildConnectionString(DbConnectionDto db);
    Task<bool> CanConnect(DbConnectionDto db, CancellationToken ct = default);
    Task CreateDatabaseIfNotExists(DbConnectionDto db, CancellationToken ct);
    Task Migrate(bool isPlatform, CancellationToken ct = default);
    Task<bool> TableExists<TEntity>(bool isPlatform, CancellationToken ct = default) where TEntity : class;
}

public class DatabaseUtility(
    TenantDbContext tenantDbContext,
    PlatformDbContext platformDbContext,
    ILogger logger) : IDatabaseUtility
{
    private readonly ILogger _logger = logger;

    private DbContext GetContext(bool isPlatform) => isPlatform ? platformDbContext : tenantDbContext;

    public async Task CreateDatabaseIfNotExists(DbConnectionDto db, CancellationToken ct)
    {
        var masterConnString = BuildConnectionString(db with { Database = "postgres" });

        await using var connection = new NpgsqlConnection(masterConnString);
        await connection.OpenAsync(ct);

        // Check if database exists
        await using (var cmd = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @database", connection))
        {
            cmd.Parameters.AddWithValue("database", db.Database);
            var exists = await cmd.ExecuteScalarAsync(ct);

            if (exists == null)
            {
                _logger.Information("Creating database: {Database}", db.Database);

                // Create database
                await using var createCmd = new NpgsqlCommand(
                    $"CREATE DATABASE {db.Database}", connection);
                await createCmd.ExecuteNonQueryAsync(ct);

                _logger.Information("Database created successfully");
            }
            else
            {
                _logger.Information("Database already exists: {Database}", db.Database);
            }
        }

        await connection.CloseAsync();
    }

    public async Task Migrate(bool isPlatform, CancellationToken ct = default)
    {
        var context = GetContext(isPlatform);
        var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();

        if (pending.Count == 0)
        {
            _logger.Information("No pending migrations for {Context}", isPlatform ? "Platform" : "Tenant");
            return;
        }

        _logger.Information("Applying {Count} migrations to {Context}...", pending.Count, isPlatform ? "Platform" : "Tenant");
        await context.Database.MigrateAsync(ct);
    }

    public async Task<bool> TableExists<TEntity>(bool isPlatform, CancellationToken ct = default) where TEntity : class
    {
        var context = GetContext(isPlatform);
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var tableName = entityType?.GetTableName();
        var schema = entityType?.GetSchema() ?? "public";

        if (string.IsNullOrEmpty(tableName)) return false;

        // Use the new EF 8.0+ SqlQuery for cleaner syntax
        var exists = await context.Database
            .SqlQueryRaw<bool>(
                "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = {0} AND table_name = {1}) AS \"Value\"",
                schema, tableName)
            .FirstOrDefaultAsync(ct);

        return exists;
    }

    public async Task<bool> CanConnect(DbConnectionDto db, CancellationToken ct = default)
    {
        try
        {
            var connString = BuildConnectionString(db) + ";Pooling=false;Timeout=10";
            await using var connection = new NpgsqlConnection(connString);

            await connection.OpenAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Database connection test failed for {Host}/{Database}", db.Host, db.Database);
            return false;
        }
    }

    public string BuildConnectionString(DbConnectionDto db) =>
        $"Host={db.Host};Port={db.Port};Database={db.Database};Username={db.User};Password={db.Pass};Include Error Detail=true";
}
