using Fringe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fringe.Domain.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rf => rf.Id);

        builder.HasOne(rf => rf.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rf => rf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}