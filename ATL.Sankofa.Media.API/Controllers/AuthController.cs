using System.Security.Claims;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ATL.Sankofa.Media.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly SignInManager<User> _signInManager;

    public AuthController(IAuthService authService, SignInManager<User> signInManager)
    {
        _authService = authService;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        if (!result.Succeeded)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("external-providers")]
    public async Task<ActionResult<IEnumerable<string>>> GetExternalProviders()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var providers = schemes.Select(s => s.Name);
        return Ok(providers);
    }

    [HttpGet("external-login/{provider}")]
    public IActionResult ExternalLogin(string provider, [FromQuery] string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<ActionResult<AuthResponse>> ExternalLoginCallback([FromQuery] string returnUrl = "/")
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return BadRequest(new AuthResponse { Errors = new List<string> { "External login information not available." } });
        }

        var externalLoginInfo = new ExternalLoginInfoModel
        {
            Provider = info.LoginProvider,
            ProviderKey = info.ProviderKey,
            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
            DisplayName = info.Principal.FindFirstValue(ClaimTypes.Name),
            AvatarUrl = info.Principal.FindFirstValue("picture")
        };

        var result = await _authService.ExternalLoginAsync(externalLoginInfo);
        if (!result.Succeeded)
            return BadRequest(result);

        // For web clients, redirect with token in query string
        return Redirect($"{returnUrl}?token={result.Token}&refreshToken={result.RefreshToken}");
    }

    [HttpPost("external-login-token")]
    public async Task<ActionResult<AuthResponse>> ExternalLoginWithToken([FromBody] ExternalLoginInfoModel loginInfo, CancellationToken cancellationToken)
    {
        // Used by MAUI clients that handle OAuth flow themselves
        var result = await _authService.ExternalLoginAsync(loginInfo, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("user-info")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            return Unauthorized();

        var userInfo = await _authService.GetUserInfoAsync(id, cancellationToken);
        if (userInfo == null)
            return NotFound();

        return Ok(userInfo);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
        {
            await _authService.RevokeRefreshTokenAsync(id);
        }

        return Ok();
    }
}
