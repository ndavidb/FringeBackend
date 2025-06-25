using Fringe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fringe.Domain.Configurations;

public class ShowConfiguration : IEntityTypeConfiguration<Show>
{
    public void Configure(EntityTypeBuilder<Show> builder)
    {
        builder.HasKey(e => e.ShowId);
        builder.Property(e => e.ShowId).HasColumnName("showid");
        builder.Property(e => e.ShowName).HasColumnName("showname").HasMaxLength(150).IsRequired();
        builder.Property(e => e.VenueId).HasColumnName("venueid");
        builder.Property(e => e.ShowTypeId).HasColumnName("showtypeid");
        builder.Property(e => e.Description).HasColumnName("description");
        builder.Property(e => e.AgeRestrictionId).HasColumnName("agerestrictionid");
        builder.Property(e => e.WarningDescription).HasColumnName("warningdescription");
        builder.Property(e => e.StartDate).HasColumnName("startdate");
        builder.Property(e => e.EndDate).HasColumnName("enddate");
        builder.Property(e => e.TicketTypeId).HasColumnName("tickettypeid");
        builder.Property(e => e.ImagesUrl).HasColumnName("imagesurl");
        builder.Property(e => e.VideosUrl).HasColumnName("videosurl");
        builder.Property(e => e.Active).HasColumnName("active").HasDefaultValue(true);
        builder.Property(e => e.CreatedById).HasColumnName("createdbyid");
        builder.Property(e => e.CreatedAt).HasColumnName("createdat");
        builder.Property(e => e.UpdatedId).HasColumnName("updatedid");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedat");
        builder.Property(e => e.ImagesUrl).IsRequired(false);
        builder.Property(e => e.VideosUrl).IsRequired(false);
        builder.Property(e => e.WarningDescription).IsRequired(false);

        // Create relationships
        builder.HasOne(e => e.Venue)
            .WithMany()
            .HasForeignKey(e => e.VenueId);

        builder.HasOne(e => e.ShowTypeLookup)
            .WithMany()
            .HasForeignKey(e => e.ShowTypeId);

        builder.HasOne(e => e.AgeRestrictionLookup)
            .WithMany()
            .HasForeignKey(e => e.AgeRestrictionId);

        builder.HasOne(e => e.TicketType)
            .WithMany()
            .HasForeignKey(e => e.TicketTypeId);
    }
}