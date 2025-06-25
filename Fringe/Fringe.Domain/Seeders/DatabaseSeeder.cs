using Fringe.Domain.Entities;
using Fringe.Domain.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Fringe.Domain.Seeders;

public class DatabaseSeeder
{
    public static async Task SeedDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FringeDbContext>();
        
        // Check if the database is created
        await AuthSeeder.SeedRolesAndAdminAsync(serviceProvider);
        await ShowsSeeder.SeedShowsDataAsync(serviceProvider);
        
    }
}