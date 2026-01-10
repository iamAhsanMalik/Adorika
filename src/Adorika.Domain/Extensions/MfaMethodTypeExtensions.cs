using Adorika.Domain.Enums;

namespace Adorika.Domain.Extensions;

/// <summary>
/// Extension methods for MfaMethodType enum to work with flags.
/// </summary>
public static class MfaMethodTypeExtensions
{
    /// <summary>
    /// Checks if a specific MFA method is included in the flags.
    /// </summary>
    public static bool HasMethod(this MfaMethodType methods, MfaMethodType method)
    {
        return (methods & method) == method;
    }

    /// <summary>
    /// Adds a method to the enabled methods.
    /// </summary>
    public static MfaMethodType AddMethod(this MfaMethodType methods, MfaMethodType method)
    {
        return methods | method;
    }

    /// <summary>
    /// Removes a method from the enabled methods.
    /// </summary>
    public static MfaMethodType RemoveMethod(this MfaMethodType methods, MfaMethodType method)
    {
        return methods & ~method;
    }

    /// <summary>
    /// Gets all individual methods from the flags.
    /// </summary>
    public static IEnumerable<MfaMethodType> GetEnabledMethods(this MfaMethodType methods)
    {
        if (methods == MfaMethodType.None)
        {
            yield break;
        }

        if (methods.HasMethod(MfaMethodType.Authenticator))
        {
            yield return MfaMethodType.Authenticator;
        }

        if (methods.HasMethod(MfaMethodType.Sms))
        {
            yield return MfaMethodType.Sms;
        }

        if (methods.HasMethod(MfaMethodType.Email))
        {
            yield return MfaMethodType.Email;
        }

        if (methods.HasMethod(MfaMethodType.BackupCode))
        {
            yield return MfaMethodType.BackupCode;
        }
    }

    /// <summary>
    /// Counts how many methods are enabled.
    /// </summary>
    public static int CountEnabledMethods(this MfaMethodType methods)
    {
        if (methods == MfaMethodType.None)
        {
            return 0;
        }

        var count = 0;
        var value = (int)methods;

        // Count set bits using Brian Kernighan's algorithm
        while (value != 0)
        {
            value &= value - 1;
            count++;
        }

        return count;
    }

    /// <summary>
    /// Checks if multiple methods are enabled.
    /// </summary>
    public static bool HasMultipleMethods(this MfaMethodType methods)
    {
        return methods.CountEnabledMethods() > 1;
    }

    /// <summary>
    /// Gets a human-readable string of enabled methods.
    /// </summary>
    public static string ToDisplayString(this MfaMethodType methods)
    {
        if (methods == MfaMethodType.None)
        {
            return "None";
        }

        var enabledMethods = methods.GetEnabledMethods()
            .Select(m => m.ToString())
            .ToArray();

        return string.Join(", ", enabledMethods);
    }

    /// <summary>
    /// Validates that the method is a single valid method (not a combination).
    /// </summary>
    public static bool IsSingleMethod(this MfaMethodType method)
    {
        return method != MfaMethodType.None && !method.HasMultipleMethods();
    }
}
