namespace MobileBackend.Framework.Security;

/// <summary>
/// Interface for password hashing and verification using BCrypt
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hash a plain text password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Hashed password</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Validate password complexity requirements
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>True if password meets requirements</returns>
    bool ValidatePasswordComplexity(string password);
}
