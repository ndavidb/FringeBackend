namespace Fringe.Domain.DTOs
{
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool CanCreate { get; set; }
        public bool CanRead { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
