# Installation Endpoints - Quick Reference Guide

## 🚀 What Changed (TL;DR)

### Critical Fixes
1. **Security**: Removed connection string from API response
2. **CQRS**: Fixed handler to use mediator instead of direct service calls
3. **Transactions**: Added database transaction for atomicity
4. **Rate Limiting**: Protected endpoints from abuse (5 req/15min)
5. **Audit Trail**: Full tracking of installation attempts

### Architectural Improvements
1. **Domain Events**: Added event infrastructure and `SystemInitializedEvent`
2. **Constants**: Replaced magic strings with `SchemaVersions.Current`
3. **Validation**: Stricter rules for tenant identifiers, passwords, database names
4. **Query Classification**: `DatabaseConnectionCommand` → `TestDatabaseConnectionQuery`

---

## 📁 File Changes Overview

### New Files
```
src/Adorika.Domain/
  ├── Events/
  │   ├── IDomainEvent.cs
  │   ├── DomainEvent.cs
  │   └── SystemInitializedEvent.cs
  ├── Constants/
  │   └── SchemaVersions.cs
  └── Entities/
      └── InstallationAudit.cs

docs/
  ├── INSTALLATION_IMPROVEMENTS.md
  └── QUICK_REFERENCE.md
```

### Renamed Files
```
TestDatabaseConnection/
  ├── DatabaseConnectionCommand.cs → TestDatabaseConnectionQuery.cs
  ├── DatabaseConnectionHandler.cs → TestDatabaseConnectionHandler.cs
  └── DatabaseConnectionValidator.cs → TestDatabaseConnectionValidator.cs
```

### Modified Files
```
src/Adorika.Domain/
  └── Entities/
      ├── Base/BaseEntity.cs (added domain events collection)
      └── SystemConfiguration.cs (raises domain event)

src/Adorika.Application/
  ├── Features/Installation/
  │   ├── InstallSystem/
  │   │   ├── InstallSystemHandler.cs (mediator, audit, transaction)
  │   │   ├── InstallSystemResponse.cs (removed connection string)
  │   │   └── InstallSystemValidator.cs (stricter validation)
  │   └── TestDatabaseConnection/
  │       ├── TestDatabaseConnectionQuery.cs (renamed)
  │       ├── TestDatabaseConnectionHandler.cs (IQueryHandler)
  │       └── TestDatabaseConnectionValidator.cs (renamed)
  ├── Common/Interfaces/Services/
  │   └── ISystemInstallation.cs (added audit params)
  └── ConfigureDependencies.cs (added HttpContextAccessor)

src/Adorika.Infrastructure/
  ├── Persistence/
  │   └── PlatformDbContext.cs (added InstallationAudit)
  └── Services/
      └── SystemInstallation.cs (transaction, audit, events)

src/Adorika.Api/
  ├── Endpoints/
  │   └── InstallationEndpoints.cs (rate limiting, renamed query)
  └── Common/Extensions/
      ├── ConfigureDependencies.cs (rate limiter config)
      └── ConfigureRequestPipeline.cs (UseRateLimiter)
```

---

## 🔧 Usage Examples

### Using Schema Version Constants
```csharp
// ❌ Before
config.MarkAsInitialized(tenantId, userId, "1.0.0");

// ✅ After
using Adorika.Domain.Constants;
config.MarkAsInitialized(tenantId, userId, SchemaVersions.Current);
```

### Domain Events
```csharp
// In domain entity
public void MarkAsInitialized(string tenantId, Guid userId, string version)
{
    // Business logic
    IsInitialized = true;
    
    // Raise domain event
    AddDomainEvent(new SystemInitializedEvent(
        Id, tenantId, userId, version, ipAddress, userAgent));
}

// In infrastructure (after SaveChanges)
foreach (var domainEvent in entity.DomainEvents)
{
    _logger.Information("Event: {Type}", domainEvent.GetType().Name);
    // Publish to message bus, etc.
}
entity.ClearDomainEvents();
```

### CQRS Pattern
```csharp
// ❌ Before - Direct service call
var status = await _systemInstallation.GetInstallationStatus(ct);

// ✅ After - Through mediator
var status = await _mediator.Send(new InstallationStatusQuery(), ct);
```

### Audit Trail
```csharp
// Automatically tracked in SystemInstallation service
var auditRecord = InstallationAudit.CreateStarted(
    tenantIdentifier, ipAddress, userAgent, dbHost, dbName);

// On success
auditRecord.MarkAsCompleted(tenantId, userId, email, version);

// On failure
auditRecord.MarkAsFailed(exception.Message);
```

---

## 🔒 Security Improvements

### Before (Insecure)
```csharp
public record InstallSystemResponse
{
    public string? EnvironmentVariable { get; init; }
}

// Response contains:
EnvironmentVariable = "ConnectionStrings__DefaultConnection=\"Host=localhost;Database=mydb;Username=admin;Password=secret123\""
```

### After (Secure)
```csharp
public record InstallSystemResponse
{
    public bool DatabaseConfigured { get; init; }
}

// Response contains:
DatabaseConfigured = true
// No credentials exposed!
```

---

## 🛡️ Rate Limiting

### Configuration
```csharp
// In ConfigureDependencies.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("installation", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;        // Max 5 requests
        limiterOptions.Window = TimeSpan.FromMinutes(15);  // Per 15 minutes
        limiterOptions.QueueLimit = 2;         // Queue up to 2 requests
    });
});
```

### Applying to Endpoints
```csharp
group.MapPost("", handler)
    .WithName("InstallSystem")
    .RequireRateLimiting("installation")  // ← Add this
    .WithLinks<InstallSystemResponse>(...);
```

---

## ✅ Validation Rules

### Tenant Identifier
```
✅ Valid:   "acme-corp", "tenant123", "my-app"
❌ Invalid: "ACME", "Tenant_123", "-tenant", "tenant-", "tenant--name"

Rules:
- 3-50 characters
- Lowercase letters, numbers, hyphens only
- Cannot start/end with hyphen
- No consecutive hyphens
```

### Password
```
Rules:
- 8-128 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

Example: "MyP@ssw0rd!"
```

### Database Name
```
✅ Valid:   "my_database", "db123", "_internal"
❌ Invalid: "123db", "my-database", "my database"

Rules:
- Max 63 characters
- Must start with letter or underscore
- Letters, numbers, underscores only
```

---

## 🗄️ Database Migration

### Generate Migration
```bash
dotnet ef migrations add AddInstallationAuditAndDomainEvents \
  -c PlatformDbContext \
  -o Migrations/Platform \
  -s src/Adorika.Api \
  -p src/Adorika.Infrastructure
```

### Apply Migration
```bash
dotnet ef database update \
  -c PlatformDbContext \
  -s src/Adorika.Api \
  -p src/Adorika.Infrastructure
```

### New Tables
- `InstallationAudits` - Tracks all installation attempts

---

## 🧪 Testing Guide

### Unit Tests to Add
```csharp
// Validation
[Fact]
public void TenantIdentifier_MustBeLowercase()
{
    var validator = new InstallSystemValidator();
    var command = new InstallSystemCommand { TenantIdentifier = "UPPERCASE" };
    var result = validator.Validate(command);
    result.IsValid.Should().BeFalse();
}

// Domain Events
[Fact]
public void MarkAsInitialized_ShouldRaiseDomainEvent()
{
    var config = new SystemConfiguration();
    config.MarkAsInitialized("tenant1", userId, SchemaVersions.Current);
    config.DomainEvents.Should().HaveCount(1);
    config.DomainEvents.First().Should().BeOfType<SystemInitializedEvent>();
}

// Audit Trail
[Fact]
public void MarkAsCompleted_ShouldCalculateDuration()
{
    var audit = InstallationAudit.CreateStarted(...);
    Thread.Sleep(100);
    audit.MarkAsCompleted(...);
    audit.DurationMs.Should().BeGreaterThan(0);
}
```

### Integration Tests to Add
```csharp
[Fact]
public async Task Installation_ShouldCreateAuditRecord()
{
    var result = await InstallSystem();
    var audit = await GetAuditRecord();
    audit.IsSuccessful.Should().BeTrue();
    audit.TenantIdentifier.Should().Be("test-tenant");
}

[Fact]
public async Task Installation_ShouldRollbackOnFailure()
{
    // Simulate failure during installation
    var result = await InstallSystemWithInvalidData();
    var tenants = await GetTenants();
    tenants.Should().BeEmpty(); // Transaction rolled back
}
```

---

## 📊 Monitoring & Observability

### Logs to Watch
```
Information: Starting System Installation for Tenant: {Tenant}
Information: Transaction started for system installation
Information: Platform data bootstrapped: Tenant={TenantId}, SuperUser={SuperUserId}
Information: Domain Event Raised: SystemInitializedEvent at {OccurredAt}
Information: System marked as initialized with schema version: {SchemaVersion}
Information: System installation completed successfully for Tenant: {Tenant}
```

### Error Logs
```
Error: Installation fatal error for Tenant: {Tenant}
Error: Failed to update audit record for failed installation
Error: Failed to rollback transaction
```

### Metrics to Track
- Installation success rate
- Installation duration (from audit table)
- Failed installation reasons
- Rate limit violations
- IP addresses of installation attempts

---

## 🚨 Troubleshooting

### "Too Many Requests" (429)
**Cause**: Rate limiting triggered (>5 requests in 15 minutes)
**Solution**: Wait 15 minutes or adjust rate limit in configuration

### "System is already initialized"
**Cause**: Installation already completed
**Solution**: Check `SystemConfiguration` table, `IsInitialized` flag

### Transaction Rollback
**Cause**: Error during installation process
**Solution**: Check `InstallationAudits` table for `ErrorMessage`

### Domain Events Not Publishing
**Cause**: Events not cleared after processing
**Solution**: Call `entity.ClearDomainEvents()` after publishing

---

## 🎯 Best Practices

### DO ✅
- Use `SchemaVersions.Current` for version numbers
- Let mediator handle all queries/commands
- Wrap multi-step operations in transactions
- Log important state changes
- Validate input strictly
- Clear domain events after processing

### DON'T ❌
- Return connection strings in responses
- Call services directly from handlers
- Use magic strings for versions
- Skip validation for "admin" operations
- Ignore rate limiting on sensitive endpoints
- Leave transactions open

---

## 📞 Support

### Questions About Changes?
1. Read full documentation: `docs/INSTALLATION_IMPROVEMENTS.md`
2. Check commit history for specific changes
3. Run diagnostics: `dotnet build`

### Found a Bug?
1. Check `InstallationAudits` table for error details
2. Review application logs
3. Verify database transaction state

---

## 🎓 Learning Resources

### CQRS Pattern
- Queries: Read operations, no side effects
- Commands: Write operations, mutate state
- All go through mediator pipeline

### Domain Events
- Capture important business occurrences
- Enable decoupled event handlers
- Provide audit trail

### Transaction Management
- Ensures atomicity (all-or-nothing)
- Prevents partial updates
- Automatic rollback on error

---

**Last Updated**: 2024
**Version**: 1.0.0
**Status**: Production Ready ✅