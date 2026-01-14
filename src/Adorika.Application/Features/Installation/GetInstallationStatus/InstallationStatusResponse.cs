namespace Adorika.Application.Features.Installation.GetInstallationStatus;

/// <summary>
/// Response model for checking installation status.
/// </summary>
public sealed record InstallationStatusResponse
{
    public bool IsInitialized { get; init; }
    public DateTime? InitializedAt { get; init; }
    public string? SchemaVersion { get; init; }

    // Static factory methods stay the same but return the record
    public static InstallationStatusResponse NotInitialized() => new()
    {
        IsInitialized = false,
    };

    public static InstallationStatusResponse AlreadyInitialized(DateTime initializedAt, string? schemaVersion) => new()
    {
        IsInitialized = true,
        InitializedAt = initializedAt,
        SchemaVersion = schemaVersion,
    };
}