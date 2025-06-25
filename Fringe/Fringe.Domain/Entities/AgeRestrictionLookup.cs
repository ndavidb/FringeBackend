namespace Fringe.Domain.Entities;

public class AgeRestrictionLookup
{
    public int AgeRestrictionId { get; set; }
    public string Code { get; set; } // e.g., "G", "PG", "M", "MA15+", "R18+"
    public string Description { get; set; }
}