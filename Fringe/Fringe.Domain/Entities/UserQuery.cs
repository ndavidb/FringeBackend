using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fringe.Domain.Entities;

// Domain entity representing the Query data
public class UserQuery
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int QueryId { get; set; }
    public string Name { get; set; }
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email{ get; set; }
    public string Message { get; set; }
    
}