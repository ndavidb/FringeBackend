using System.ComponentModel.DataAnnotations;

namespace Fringe.Domain.DTOs.AuthDTOs;

public class RefreshTokenDto
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;
        
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}