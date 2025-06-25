using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Fringe.Domain;
using Fringe.Domain.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Fringe.Service;

public class AuthService : IAuthService
{
    private readonly IEmailService _emailService;
    private readonly IAuthRepository _authRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AuthService(IEmailService emailService, IAuthRepository authRepository,
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _emailService = emailService;
        _authRepository = authRepository;
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _roleManager = roleManager;
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto model)
    {
        var user = await _authRepository.FindUserByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return null;

        if (!user.IsActive)
            throw new InvalidOperationException("Your account is not active. Please contact support.");

        var userRoles = await _authRepository.GetUserRolesAsync(user);
        var tokenResponse = GenerateTokens(user, userRoles);

        var refreshToken = new RefreshToken
        {
            Token = tokenResponse.RefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _authRepository.SaveRefreshTokenAsync(refreshToken);

        return tokenResponse;
    }

    // TODO: Talk to DEV Team about the approach for handling user registration different to admin registration. It can be a separate method or a flag in the RegisterDto.
    public async Task<IdentityResult> RegisterAsync(RegisterDto model)
    {
        var existingUser = await _authRepository.FindUserByEmailAsync(model.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true, // Check with DEV Team about confirmation email for registration
            IsActive = true
        };

        var results = await _userManager.CreateAsync(user, model.Password);
        if (!results.Succeeded)
            return results;

        await _authRepository.AddUserToRoleAsync(user, "User");

        return IdentityResult.Success;
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenDto model)
    {
        var principal = GetPrincipalFromExpiredToken(model.AccessToken);
        if (principal == null)
            return null;

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var refreshToken = await _authRepository.GetRefreshTokenAsync(model.RefreshToken);

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
            return null;

        var user = await _authRepository.FindUserByIdAsync(userId);
        if (user == null || !user.IsActive)
            return null;

        await _authRepository.RevokeRefreshTokenAsync(model.RefreshToken);

        var userRoles = await _authRepository.GetUserRolesAsync(user);
        var tokenResponse = GenerateTokens(user, userRoles);

        var newRefreshToken = new RefreshToken
        {
            Token = tokenResponse.RefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _authRepository.SaveRefreshTokenAsync(newRefreshToken);
        return tokenResponse;
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        return await _authRepository.RevokeRefreshTokenAsync(token);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto model)
    {
        var user = await _authRepository.FindUserByEmailAsync(model.Email);
        if (user == null)
            return false;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: Send email with the token to the user. Check with DEV Team about the email service.
        await _emailService.SendEmailAsync("prince0991@outlook.com", "Test", "Test");
        return true;
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await _authRepository.FindUserByEmailAsync(model.Email);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        return await _authRepository.ResetPasswordAsync(user, model.Token, model.NewPassword);

    }

    public async Task<IdentityResult> CreateManagerAsync(RegisterDto model, string creatorUserId)
    {
        // Verify the creator is an admin
        var creator = await _authRepository.FindUserByIdAsync(creatorUserId);
        if (creator == null)
            return IdentityResult.Failed(new IdentityError { Description = "Creator not found." });

        var isAdmin = await _authRepository.IsInRoleAsync(creator, "Admin");
        if (!isAdmin)
            return IdentityResult.Failed(new IdentityError { Description = "Only administrators can create manager accounts." });

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _authRepository.CreateUserAsync(user, model.Password);
        if (!result.Succeeded)
            return result;

        return await _authRepository.AddUserToRoleAsync(user, "Manager");
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string role)
    {
        return await _userManager.GetUsersInRoleAsync(role);
    }

    private TokenResponseDto GenerateTokens(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            Roles = roles,
            UserId = user.Id.ToString(),
            Email = user.Email ?? string.Empty
        };
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false, // We need to validate expired tokens
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
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

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        return await _authRepository.FindUserByEmailAsync(email);
    }

    public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
    {
        return await _authRepository.FindUserByIdAsync(userId);
    }
}
