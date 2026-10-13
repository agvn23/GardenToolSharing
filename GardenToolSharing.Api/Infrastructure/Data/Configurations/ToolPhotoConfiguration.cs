using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenToolSharing.Api.Infrastructure.Data.Configurations;

public class ToolPhotoConfiguration : IEntityTypeConfiguration<ToolPhoto>
{
    public void Configure(EntityTypeBuilder<ToolPhoto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Caption).IsRequired().HasMaxLength(100);
        builder.Property(p => p.StoredFileName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.ContentType).IsRequired().HasMaxLength(50);

        builder.HasOne(p => p.Tool)
            .WithMany(t => t.Photos)
            .HasForeignKey(p => p.ToolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ToolId);
    }
}