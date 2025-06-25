using Fringe.Domain.Configurations;
using Fringe.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fringe.Domain
{
    public class FringeDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public FringeDbContext(DbContextOptions<FringeDbContext> options) : base(options)
        {
        }

        public DbSet<Show> Shows { get; set; }
        public DbSet<ShowTypeLookup> ShowTypeLookups { get; set; }
        public DbSet<AgeRestrictionLookup> AgeRestrictionLookups { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }

        public DbSet<TicketPrice> TicketPrices { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        // Venues DbSet
        public DbSet<Venue> Venues { get; set; }

        // SeatingPlan DbSet
        public DbSet<SeatingPlan> SeatingPlans { get; set; }

        // Location DbSet
        public DbSet<Location> Locations { get; set; }

        // VenueTypes DbSet
        public DbSet<VenueTypeLookup> VenueTypeLookUps { get; set; }

        public DbSet<UserQuery> UserQueries { get; set; }

        public DbSet<Performance> Performances { get; set; }
        public DbSet<ReservedSeat> ReservedSeats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<TicketPrice>(entity =>
            {
                entity.HasKey(tp => tp.TicketPriceId);
                entity.Property(tp => tp.Price).HasColumnType("decimal(18,2)");

                entity.HasOne(tp => tp.TicketType)
                    .WithMany(tt => tt.TicketPrices)
                    .HasForeignKey(tp => tp.TicketTypeId);

                entity.HasOne(tp => tp.Performance)
                    .WithMany(p => p.TicketPrices)
                    .HasForeignKey(tp => tp.PerformanceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReservedSeat>(entity =>
            {
                entity.HasKey(e => e.ReservedSeatId);

                entity.HasOne(e => e.Ticket)
                    .WithMany()
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.SeatingPlan)
                    .WithMany(sp => sp.ReservedSeat)
                    .HasForeignKey(e => e.SeatingPlanId);
            });

            //Configures Location entity.
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.LocationId);
                entity.Property(e => e.LocationId).HasColumnName("locationid");
                entity.Property(e => e.LocationName).HasColumnName("locationname").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.Suburb).HasColumnName("suburb");
                entity.Property(e => e.PostalCode).HasColumnName("postalcode");
                entity.Property(e => e.State).HasColumnName("state");
                entity.Property(e => e.Country).HasColumnName("country");
                entity.Property(e => e.Latitude).HasColumnName("latitude");
                entity.Property(e => e.Longitude).HasColumnName("longitude");
                entity.Property(e => e.ParkingAvailable).HasColumnName("parkingavailable").HasDefaultValue(false);
                entity.Property(e => e.Active).HasColumnName("active").HasDefaultValue(true);
                entity.Property(e => e.CreatedById).HasColumnName("createdbyid");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.Property(e => e.UpdatedId).HasColumnName("updatedid");
                entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
                entity.Property(e => e.Suburb).HasColumnName("suburb").IsRequired(false);
                entity.Property(e => e.PostalCode).HasColumnName("postalcode").IsRequired(false);
                entity.Property(e => e.State).HasColumnName("state").IsRequired(false);
                entity.Property(e => e.Country).HasColumnName("country").IsRequired(false);
            });

            // Configure VenueTypeLookUp entity
            modelBuilder.Entity<VenueTypeLookup>(entity =>
            {
                entity.ToTable("VenueTypeLookup"); // explicitly set the table name
                entity.HasKey(e => e.TypeId);
                entity.Property(e => e.TypeId).HasColumnName("typeid");
                entity.Property(e => e.VenueType).HasColumnName("venuetype").HasMaxLength(100).IsRequired();
            });

            // Configures Venue entity.
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(e => e.VenueId);
                entity.Property(e => e.VenueId).HasColumnName("venueid");
                entity.Property(e => e.VenueName).HasColumnName("venuename").HasMaxLength(150).IsRequired();
                entity.Property(e => e.LocationId).HasColumnName("locationid");
                entity.Property(e => e.TypeId).HasColumnName("typeid");
                entity.Property(e => e.MaxCapacity).HasColumnName("maxcapacity");
                // entity.Property(e => e.SeatingPlanId).HasColumnName("seatingplanid");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ContactEmail).HasColumnName("contactemail");
                entity.Property(e => e.ContactPhone).HasColumnName("contactphone");
                entity.Property(e => e.IsAccessible).HasColumnName("isaccessible").HasDefaultValue(false);
                entity.Property(e => e.VenueUrl).HasColumnName("venueurl");
                entity.Property(e => e.ImagesUrl).HasColumnName("imagesurl");
                entity.Property(e => e.Active).HasColumnName("active").HasDefaultValue(true);
                entity.Property(e => e.CreatedById).HasColumnName("createdbyid");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.Property(e => e.UpdatedId).HasColumnName("updatedid");
                entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");


                entity.HasOne(e => e.Location)
                    .WithMany(l => l.Venues)
                    .HasForeignKey(e => e.LocationId);

                entity.HasOne(v => v.SeatingPlan)
                    .WithOne(v => v.Venue)
                    .HasForeignKey<SeatingPlan>(v => v.VenueId)
                    .IsRequired();

                entity.HasOne(e => e.VenueTypeLookUp)
                    .WithMany()
                    .HasForeignKey(e => e.TypeId);
            });

            // Configures Performance entity.
            modelBuilder.Entity<Performance>(entity =>
            {
                entity.HasKey(e => e.PerformanceId);
                entity.Property(e => e.PerformanceId).HasColumnName("performanceid");
                entity.Property(e => e.ShowId).HasColumnName("showid").IsRequired();
                entity.Property(e => e.PerformanceDate).HasColumnName("performancedate").IsRequired();
                entity.Property(e => e.StartTime).HasColumnName("starttime").IsRequired();
                entity.Property(e => e.EndTime).HasColumnName("endtime").IsRequired();
                entity.Property(e => e.SoldOut).HasColumnName("soldout").HasDefaultValue(false);
                entity.Property(e => e.Cancel).HasColumnName("cancel").HasDefaultValue(false);
                entity.Property(e => e.Active).HasColumnName("active").HasDefaultValue(true);
                entity.Property(e => e.CreatedById).HasColumnName("createdbyid").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("createdat").IsRequired();
                entity.Property(e => e.UpdatedId).HasColumnName("updatedid");
                entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");

                entity.HasOne(e => e.Show)
                    .WithMany()
                    .HasForeignKey(e => e.ShowId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configures TicketType entity.
            modelBuilder.Entity<TicketType>(entity =>
            {
                entity.HasKey(e => e.TicketTypeId);
                entity.Property(e => e.TicketTypeId).HasColumnName("tickettypeid");
                entity.Property(e => e.TypeName).HasColumnName("typename").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");

                // entity.HasMany(e => e.Tickets) // Note: This should be plural if it's a collection
                //     .WithOne(t => t.TicketType)
                //     .HasForeignKey(t => t.TicketTypeId)
                //     .HasConstraintName("FK_Tickets_TicketTypes_tickettypeid");
            });


            // Configures Ticket entity.
            modelBuilder.Entity<Ticket>(entity =>
            {

                entity.HasKey(e => e.TicketId);
                entity.Property(e => e.TicketId).HasColumnName("ticketid");
                entity.Property(e => e.PerformanceId).HasColumnName("performanceid").IsRequired();
                entity.Property(e => e.UserId).HasColumnName("userid").IsRequired();
                entity.Property(e => e.QRImageUrl).HasColumnName("qrimageurl");
                entity.Property(e => e.QRInCode).HasColumnName("qrincode");
                entity.Property(e => e.StartTime).HasColumnName("starttime");
                entity.Property(e => e.EndTime).HasColumnName("endtime");
                entity.Property(e => e.IsCheckedIn).HasColumnName("ischeckedin").HasDefaultValue(false);
                entity.Property(e => e.Cancelled).HasColumnName("cancelled").HasDefaultValue(false);
                entity.Property(e => e.CreatedById).HasColumnName("createdbyid");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.Property(e => e.UpdatedId).HasColumnName("updatedid");
                entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
                entity.Property(e => e.TicketPriceId).HasColumnName("ticketpriceid");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.Price).HasColumnName("price");



                entity.HasOne(e => e.Performance)
                    .WithMany(e => e.Tickets)
                    .HasForeignKey(e => e.PerformanceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ReservedSeats)
                    .WithOne(ts => ts.Ticket)
                    .HasForeignKey(ts => ts.TicketId);
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FringeDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
