using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ATL.Sankofa.Media.Business.Configuration;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ATL.Sankofa.Media.Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<User> userManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return new AuthResponse { Errors = new List<string> { "Passwords do not match." } };
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponse { Errors = result.Errors.Select(e => e.Description).ToList() };
        }

        await _userManager.AddToRoleAsync(user, "User");
        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse { Errors = new List<string> { "Invalid email or password." } };
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            if (await _userManager.IsLockedOutAsync(user))
                return new AuthResponse { Errors = new List<string> { "Account is locked out. Please try again later." } };

            await _userManager.AccessFailedAsync(user);
            return new AuthResponse { Errors = new List<string> { "Invalid email or password." } };
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> ExternalLoginAsync(ExternalLoginInfoModel loginInfo, CancellationToken cancellationToken = default)
    {
        // Try to find user by external login
        var user = await _userManager.FindByLoginAsync(loginInfo.Provider, loginInfo.ProviderKey);

        if (user == null)
        {
            // Try to find by email
            if (!string.IsNullOrEmpty(loginInfo.Email))
            {
                user = await _userManager.FindByEmailAsync(loginInfo.Email);
            }

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    UserName = loginInfo.Email ?? $"{loginInfo.Provider}_{loginInfo.ProviderKey}",
                    Email = loginInfo.Email ?? $"{loginInfo.ProviderKey}@{loginInfo.Provider.ToLower()}.external",
                    DisplayName = loginInfo.DisplayName ?? loginInfo.Provider + " User",
                    AvatarUrl = loginInfo.AvatarUrl,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new AuthResponse { Errors = createResult.Errors.Select(e => e.Description).ToList() };
                }

                await _userManager.AddToRoleAsync(user, "User");
            }

            // Link external login
            var addLoginResult = await _userManager.AddLoginAsync(user,
                new UserLoginInfo(loginInfo.Provider, loginInfo.ProviderKey, loginInfo.Provider));

            if (!addLoginResult.Succeeded)
            {
                return new AuthResponse { Errors = addLoginResult.Errors.Select(e => e.Description).ToList() };
            }
        }

        if (!user.IsActive)
        {
            return new AuthResponse { Errors = new List<string> { "Account is deactivated." } };
        }

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
        {
            return new AuthResponse { Errors = new List<string> { "Invalid token." } };
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AuthResponse { Errors = new List<string> { "Invalid token." } };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return new AuthResponse { Errors = new List<string> { "User not found or inactive." } };
        }

        // Validate refresh token stored in user's security stamp or authentication token
        var storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
        if (storedRefreshToken != request.RefreshToken)
        {
            return new AuthResponse { Errors = new List<string> { "Invalid refresh token." } };
        }

        return await GenerateAuthResponse(user);
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList(),
            HasChannel = user.Channel != null,
            ChannelId = user.Channel?.Id
        };
    }

    public async Task<bool> RevokeRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");
        return true;
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token
        await _userManager.SetAuthenticationTokenAsync(user, "Default", "RefreshToken", refreshToken);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        return new AuthResponse
        {
            Succeeded = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles.ToList()
            }
        };
    }

    private string GenerateJwtToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("display_name", user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false // Allow expired tokens for refresh
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
