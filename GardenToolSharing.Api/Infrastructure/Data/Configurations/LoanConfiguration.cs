using GardenToolSharing.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenToolSharing.Api.Infrastructure.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Note).HasMaxLength(500);

        // Loan history is kept: deleting a tool or user with loans is blocked
        builder.HasOne(l => l.Tool)
            .WithMany(t => t.Loans)
            .HasForeignKey(l => l.ToolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Borrower)
            .WithMany(u => u.Loans)
            .HasForeignKey(l => l.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict);

        // At most one active (not yet returned) loan per tool
        builder.HasIndex(l => l.ToolId)
            .IsUnique()
            .HasFilter("\"ReturnedAt\" IS NULL");
    }
}
