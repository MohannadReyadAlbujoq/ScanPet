using MobileBackend.Application.Interfaces;
using FrameworkPasswordService = MobileBackend.Framework.Security.IPasswordService;

namespace MobileBackend.Infrastructure.Services;

/// <summary>
/// Wrapper to adapt Framework PasswordService to Application IPasswordService interface
/// </summary>
public class PasswordServiceWrapper : IPasswordService
{
    private readonly FrameworkPasswordService _passwordService;

    public PasswordServiceWrapper(FrameworkPasswordService passwordService)
    {
        _passwordService = passwordService;
    }

    public string HashPassword(string password)
    {
        return _passwordService.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return _passwordService.VerifyPassword(password, hash);
    }
}
