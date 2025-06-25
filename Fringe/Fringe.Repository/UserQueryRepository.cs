namespace Fringe.Repository;

// Handles User query data access logic
public class UserQueryRepository : IUserQueryRepository
{
    private readonly FringeDbContext _context;

    public UserQueryRepository (FringeDbContext context)
    {
        _context = context;
    }

    public async Task<UserQuery> GetShowByIdAsync(int queryId)
    {
        return await _context.UserQueries.FindAsync(queryId) ?? throw new InvalidOperationException();
    }

    public async Task<UserQuery> CreateUserQueryAsync(UserQuery query)
    {
        await _context.UserQueries.AddAsync(query);
        await _context.SaveChangesAsync();
        return query;
    }
}