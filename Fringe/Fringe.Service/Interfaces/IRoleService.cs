
namespace Fringe.Service.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int roleId);
        Task<RoleDto> CreateRoleAsync(RoleDto createRoleDto);
        Task<RoleDto> UpdateRoleAsync(int roleId, RoleDto updateRoleDto);
        Task<bool> DeleteRoleAsync(int roleId);
    }
}
