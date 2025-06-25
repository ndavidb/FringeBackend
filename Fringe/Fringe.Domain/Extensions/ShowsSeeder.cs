using Fringe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fringe.Domain.Extensions;

public class ShowsSeeder
{
    public static async Task SeedShowsDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FringeDbContext>();

        await SeedAgeRestrictionsAsync(context);
        await SeedShowTypesAsync(context);
        await SeedTicketTypesAsync(context);
    }

    private static async Task SeedAgeRestrictionsAsync(FringeDbContext context)
    {
        if (!await context.AgeRestrictionLookups.AnyAsync())
        {
            var ageRestrictions = new List<AgeRestrictionLookup>
            {
                new AgeRestrictionLookup { Code = "G", Description = "General - Suitable for all ages" },
                new AgeRestrictionLookup { Code = "PG", Description = "Parental Guidance - Mild content, parental guidance recommended" },
                new AgeRestrictionLookup { Code = "M", Description = "Mature - Recommended for mature audiences 15 years and over" },
                new AgeRestrictionLookup { Code = "MA15+", Description = "Mature Accompanied - Restricted to 15 years and over unless accompanied by parent/guardian" },
                new AgeRestrictionLookup { Code = "R18+", Description = "Restricted - Restricted to 18 years and over" }
            };

            await context.AgeRestrictionLookups.AddRangeAsync(ageRestrictions);
            await context.SaveChangesAsync();
            Console.WriteLine("Age restrictions seeded successfully");
        }
    }

    private static async Task SeedShowTypesAsync(FringeDbContext context)
    {
        if (!await context.ShowTypeLookups.AnyAsync())
        {
            var showTypes = new List<ShowTypeLookup>
            {
                new ShowTypeLookup { ShowType = "Comedy" },
                new ShowTypeLookup { ShowType = "Drama" },
                new ShowTypeLookup { ShowType = "Music" },
                new ShowTypeLookup { ShowType = "Dance" },
                new ShowTypeLookup { ShowType = "Theatre" },
                new ShowTypeLookup { ShowType = "Cabaret" },
                new ShowTypeLookup { ShowType = "Circus" },
                new ShowTypeLookup { ShowType = "Visual Arts" },
                new ShowTypeLookup { ShowType = "Film" },
                new ShowTypeLookup { ShowType = "Interactive" }
            };

            await context.ShowTypeLookups.AddRangeAsync(showTypes);
            await context.SaveChangesAsync();
            Console.WriteLine("Show types seeded successfully");
        }
    }

    private static async Task SeedTicketTypesAsync(FringeDbContext context)
    {
        if (!await context.TicketTypes.AnyAsync())
        {
            var ticketTypes = new List<TicketType>
            {
                new TicketType { TypeName = "Standard", Description = "Standard admission ticket" },
                new TicketType { TypeName = "Concession", Description = "Discounted tickets for students with valid ID" },
                new TicketType { TypeName = "Senior", Description = "Discounted tickets for seniors" },
                new TicketType { TypeName = "Children", Description = "Discounted tickets for children" }
            };

            await context.TicketTypes.AddRangeAsync(ticketTypes);
            await context.SaveChangesAsync();
            Console.WriteLine("Ticket types seeded successfully");
        }
    }
}