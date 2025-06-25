namespace Fringe.Service.Interfaces;

public interface IUserQueryService
{
    Task<UserQueryDto> CreateUserQueryAsync(UserQueryDto queryDto);
}