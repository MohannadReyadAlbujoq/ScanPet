namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Password service interface for application layer
/// Implementation is in Framework layer
/// </summary>
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
