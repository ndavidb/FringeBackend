using Fringe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fringe.Domain.Configurations;

public class AgeRestrictionLookupConfiguration : IEntityTypeConfiguration<AgeRestrictionLookup>
{
    public void Configure(EntityTypeBuilder<AgeRestrictionLookup> builder)
    {
        builder.ToTable("AgeRestrictionLookup");
        builder.HasKey(e => e.AgeRestrictionId);
        builder.Property(e => e.AgeRestrictionId).HasColumnName("agerestrictionid");
        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(200);
    }
}