using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenToolSharing.Api.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.PasswordHash).IsRequired();

        // NOCASE so "Ann@x.com" and "ann@x.com" count as the same email in SQLite
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254).UseCollation("NOCASE");
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
