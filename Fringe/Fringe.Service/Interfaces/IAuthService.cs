using Fringe.Domain.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Identity;

namespace Fringe.Service.Interfaces;

public interface IAuthService
{
    Task<TokenResponseDto?> LoginAsync(LoginDto model);
    Task<IdentityResult> RegisterAsync(RegisterDto model);
    Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenDto model);
    Task<bool> RevokeTokenAsync(string token);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto model);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model);
    Task<IdentityResult> CreateManagerAsync(RegisterDto model, string creatorUserId);
    Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string role);
    Task<ApplicationUser?> FindUserByEmailAsync(string email);
    Task<ApplicationUser?> FindUserByIdAsync(string userId);
}