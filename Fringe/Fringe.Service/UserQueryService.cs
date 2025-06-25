namespace Fringe.Service;

public class UserQueryService : IUserQueryService
{
    private readonly IUserQueryRepository _userQueryRepository;

    public UserQueryService(IUserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }
    
    public async Task<UserQueryDto> CreateUserQueryAsync(UserQueryDto queryDto)
    {
        var userQuery = new UserQuery
        {
            Name = queryDto.Name,
            Email = queryDto.Email,
            Message = queryDto.Message
        };

        var createdQuery = await _userQueryRepository.CreateUserQueryAsync(userQuery);
        return MapToDto(createdQuery);
    }
    
    //helper method to map dto
    private static UserQueryDto MapToDto(UserQuery userQuery)
    {
        return new UserQueryDto
        {
            
            Name = userQuery.Name,
            Email = userQuery.Email,
            Message = userQuery.Message
        };
    }

   
}