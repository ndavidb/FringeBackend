using Fringe.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fringe.Tests.TestHelpers;

public class DbContextHelper
{
    public static FringeDbContext CreateInMemoryContext(string dbName = null!)
    {
        dbName ??= $"FringeTestDb_{Guid.NewGuid()}";
            
        var options = new DbContextOptionsBuilder<FringeDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new FringeDbContext(options);
    }
}