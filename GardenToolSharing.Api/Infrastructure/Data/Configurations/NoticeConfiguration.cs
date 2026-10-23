using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenToolSharing.Api.Infrastructure.Data.Configurations;

public class NoticeConfiguration : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(2000);

        builder.HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.CreatedAt);
    }
}

public class NoticeCommentConfiguration : IEntityTypeConfiguration<NoticeComment>
{
    public void Configure(EntityTypeBuilder<NoticeComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).IsRequired().HasMaxLength(1000);

        builder.HasOne(c => c.Notice)
            .WithMany(n => n.Comments)
            .HasForeignKey(c => c.NoticeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.NoticeId);
    }
}