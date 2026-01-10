namespace Adorika.Domain.Enums;

/// <summary>
/// Represents the type of multi-factor authentication method.
/// Uses [Flags] to allow multiple methods to be enabled simultaneously.
/// </summary>
[Flags]
public enum MfaMethodType
{
    /// <summary>
    /// No MFA method enabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// Time-based One-Time Password (TOTP) using an authenticator app.
    /// </summary>
    Authenticator = 1 << 0, // 1

    /// <summary>
    /// SMS-based verification code.
    /// </summary>
    Sms = 1 << 1, // 2

    /// <summary>
    /// Email-based verification code.
    /// </summary>
    Email = 1 << 2, // 4

    /// <summary>
    /// Backup/recovery codes for account recovery.
    /// </summary>
    BackupCode = 1 << 3 // 8
}
