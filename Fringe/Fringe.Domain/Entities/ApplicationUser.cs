using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Fringe.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
       
}