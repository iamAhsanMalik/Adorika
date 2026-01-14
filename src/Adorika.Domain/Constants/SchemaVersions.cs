namespace Adorika.Domain.Constants;

/// <summary>
/// Database schema version constants.
/// Used to track database migrations and system upgrades.
/// </summary>
public static class SchemaVersions
{
    /// <summary>
    /// Initial release version.
    /// </summary>
    public const string V1_0_0 = "1.0.0";

    /// <summary>
    /// Current schema version.
    /// </summary>
    public const string Current = V1_0_0;

    /// <summary>
    /// Gets all available schema versions in chronological order.
    /// </summary>
    public static readonly IReadOnlyList<string> AllVersions = new[]
    {
        V1_0_0
    };

    /// <summary>
    /// Validates if a schema version is recognized.
    /// </summary>
    public static bool IsValid(string version)
    {
        return AllVersions.Contains(version);
    }
}
