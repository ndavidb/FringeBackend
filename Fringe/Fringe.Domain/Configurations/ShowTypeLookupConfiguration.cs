using Fringe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fringe.Domain.Configurations;

public class ShowTypeLookupConfiguration : IEntityTypeConfiguration<ShowTypeLookup>
{
    public void Configure(EntityTypeBuilder<ShowTypeLookup> builder)
    {
        builder.ToTable("ShowTypeLookup");
        builder.HasKey(e => e.TypeId);
        builder.Property(e => e.TypeId).HasColumnName("typeid");
        builder.Property(e => e.ShowType).HasColumnName("showtype").HasMaxLength(100).IsRequired();
    }
}