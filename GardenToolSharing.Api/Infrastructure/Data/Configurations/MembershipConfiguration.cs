using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenToolSharing.Api.Infrastructure.Data.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Tool)
            .WithMany(t => t.Memberships)
            .HasForeignKey(m => m.ToolId)
            .OnDelete(DeleteBehavior.Cascade);

        // A user can only have one membership per tool
        builder.HasIndex(m => new { m.UserId, m.ToolId }).IsUnique();
    }
}
