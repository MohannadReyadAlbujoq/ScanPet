using MobileBackend.Application.Interfaces;
using FrameworkJwtService = MobileBackend.Framework.Security.IJwtService;

namespace MobileBackend.Infrastructure.Services;

/// <summary>
/// Wrapper to adapt Framework JwtService to Application IJwtService interface
/// </summary>
public class JwtServiceWrapper : IJwtService
{
    private readonly FrameworkJwtService _jwtService;

    public JwtServiceWrapper(FrameworkJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public string GenerateAccessToken(Guid userId, string username, string email, List<string> roles, long permissionsBitmask)
    {
        // Application interface includes username, but Framework service doesn't need it for token generation
        return _jwtService.GenerateAccessToken(userId, email, roles, permissionsBitmask);
    }

    public string GenerateRefreshToken()
    {
        return _jwtService.GenerateRefreshToken();
    }

    public bool ValidateToken(string token)
    {
        var result = _jwtService.ValidateToken(token);
        return result.IsValid;
    }
}
