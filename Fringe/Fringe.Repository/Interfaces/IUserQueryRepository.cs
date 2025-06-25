namespace Fringe.Repository.Interfaces;

public interface IUserQueryRepository
{
    Task<UserQuery> CreateUserQueryAsync(UserQuery query);
}