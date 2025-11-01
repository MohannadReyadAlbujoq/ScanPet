using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MobileBackend.Framework.Security.Models;

namespace MobileBackend.Framework.Security;

/// <summary>
/// Implementation of JWT token service using RS256 algorithm
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly RSA _privateRsa;
    private readonly RSA _publicRsa;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        _tokenHandler = new JwtSecurityTokenHandler();

        // Load RSA keys from base64-encoded XML strings
        _privateRsa = new RSACryptoServiceProvider(2048);
        _publicRsa = new RSACryptoServiceProvider(2048);

        try
        {
            // Decode Base64 to XML format
            var privateKeyBytes = Convert.FromBase64String(_jwtSettings.PrivateKey);
            var privateKeyXml = Encoding.UTF8.GetString(privateKeyBytes);
            ((RSACryptoServiceProvider)_privateRsa).FromXmlString(privateKeyXml);

            var publicKeyBytes = Convert.FromBase64String(_jwtSettings.PublicKey);
            var publicKeyXml = Encoding.UTF8.GetString(publicKeyBytes);
            ((RSACryptoServiceProvider)_publicRsa).FromXmlString(publicKeyXml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load RSA keys. Ensure they are properly base64-encoded XML format. Run 'generate-jwt-keys.ps1' to generate keys.", ex);
        }
    }

    public string GenerateAccessToken(Guid userId, string email, List<string> roles, long permissionsBitmask)
    {
        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(_privateRsa),
            SecurityAlgorithms.RsaSha256
        );

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("permissions", permissionsBitmask.ToString())
        };

        // Add roles as separate claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = signingCredentials,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Generate a cryptographically secure random refresh token
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    public JwtValidationResult ValidateToken(string token)
    {
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new RsaSecurityKey(_publicRsa),
                ClockSkew = TimeSpan.Zero // No tolerance for expiration
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var result = new JwtValidationResult
            {
                IsValid = true
            };

            // Extract claims
            if (Guid.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId))
            {
                result.UserId = userId;
            }

            result.Email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            result.Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (long.TryParse(principal.FindFirst("permissions")?.Value, out var permissions))
            {
                result.PermissionsBitmask = permissions;
            }

            return result;
        }
        catch (SecurityTokenExpiredException)
        {
            return new JwtValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token has expired"
            };
        }
        catch (Exception ex)
        {
            return new JwtValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Token validation failed: {ex.Message}"
            };
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var result = ValidateToken(token);
        return result.IsValid ? result.UserId : null;
    }
}
