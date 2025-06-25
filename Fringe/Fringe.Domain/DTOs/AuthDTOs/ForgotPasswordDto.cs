using System.ComponentModel.DataAnnotations;

namespace Fringe.Domain.DTOs.AuthDTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}