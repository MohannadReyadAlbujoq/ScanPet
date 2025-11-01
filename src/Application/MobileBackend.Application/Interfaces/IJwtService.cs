namespace MobileBackend.Application.Interfaces;

/// <summary>
/// JWT service interface for application layer
/// Implementation is in Framework layer
/// </summary>
public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string username, string email, List<string> roles, long permissionsBitmask);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}
