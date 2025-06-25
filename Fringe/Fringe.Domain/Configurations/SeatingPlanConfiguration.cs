using Fringe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fringe.Domain.Configurations;

public class SeatingPlanConfiguration : IEntityTypeConfiguration<SeatingPlan>
{
    public void Configure(EntityTypeBuilder<SeatingPlan> builder)
    {
        builder.HasKey(e => e.SeatingPlanId);
        builder.Property(e => e.SeatingPlanId).HasColumnName("seatingplanid");
        builder.Property(e => e.VenueId).HasColumnName("venueid");
        builder.Property(e => e.Rows).HasColumnName("rows");
        builder.Property(e => e.SeatsPerRow).HasColumnName("seatsperrow");
        builder.Property(e => e.CreatedAt).HasColumnName("createdat");
        builder.Property(e => e.CreatedById).HasColumnName("createdbyid");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedat");
        
    }
}