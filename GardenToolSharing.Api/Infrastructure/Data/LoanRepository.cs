using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GardenToolSharing.Api.Infrastructure.Data;

public class LoanRepository(AppDbContext db) : ILoanRepository
{
    public async Task AddAsync(Loan loan, CancellationToken ct)
    {
        db.Loans.Add(loan);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        // SQLite error 19 = constraint violation: the tool's one-active-loan index was raced
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            throw new ConflictException("This tool already has an active loan.");
        }
    }

    public Task<Loan?> FindByIdAsync(int id, CancellationToken ct) =>
        db.Loans
            .Include(l => l.Tool)
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<Loan?> FindActiveByToolIdAsync(int toolId, CancellationToken ct) =>
        db.Loans
            .Include(l => l.Tool)
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.ToolId == toolId && l.ReturnedAt == null, ct);

    public void Remove(Loan loan) => db.Loans.Remove(loan);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
