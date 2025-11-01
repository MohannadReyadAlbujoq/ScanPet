using System.Text.RegularExpressions;

namespace MobileBackend.Framework.Security;

/// <summary>
/// Implementation of password hashing and verification using BCrypt
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // BCrypt work factor (cost parameter)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be null or empty", nameof(hash));

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public bool ValidatePasswordComplexity(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Password requirements:
        // - Minimum 8 characters
        // - Maximum 128 characters
        // - At least one uppercase letter
        // - At least one lowercase letter
        // - At least one digit
        // - At least one special character

        if (password.Length < 8 || password.Length > 128)
            return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[^a-zA-Z0-9]");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }
}
