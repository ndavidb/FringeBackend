namespace Fringe.Domain.DTOs.AuthDTOs;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}