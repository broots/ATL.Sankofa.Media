using ATL.Sankofa.Media.Business.Models;

namespace ATL.Sankofa.Media.Business.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> ExternalLoginAsync(ExternalLoginInfoModel loginInfo, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<UserInfoDto?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}
