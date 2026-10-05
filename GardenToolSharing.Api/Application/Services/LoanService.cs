using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Common.Exceptions;
using GardenToolSharing.Api.Dtos.Loans;
using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Services;

public class LoanService(
    ILoanRepository loans,
    IToolRepository tools,
    IMembershipRepository memberships,
    TimeProvider time) : ILoanService
{
    public async Task<LoanDto> CreateAsync(int borrowerId, CreateLoanRequest request, CancellationToken ct)
    {
        var toolId = request.ToolId!.Value;
        var now = time.GetUtcNow().UtcDateTime;

        // Tracked, so setting Status below is saved together with the new loan in one SaveChanges
        var tool = await tools.FindTrackedByIdAsync(toolId, ct)
                   ?? throw new NotFoundException("Tool not found.");

        var membership = await memberships.FindByUserAndToolAsync(borrowerId, toolId, ct);
        if (membership is null || membership.Status != MembershipStatus.Active)
            throw new ForbiddenException("You must be an approved member of this tool to borrow it.");

        if (tool.Status != ToolStatus.Available)
            throw new ConflictException("This tool is already lent out.");

        var from = request.BorrowedFrom ?? DateOnly.FromDateTime(now);
        var until = request.BorrowedUntil!.Value;

        if (until <= from)
            throw DomainValidationException.ForField("borrowedUntil", "Borrowed until must be after borrowed from.");

        if (!tool.IsWithinWindow(from, until))
        {
            throw DomainValidationException.ForField(
                "borrowedUntil", "The loan period must fall within the tool's availability window.");
        }

        var loan = new Loan
        {
            ToolId = toolId,
            BorrowerId = borrowerId,
            BorrowedFrom = from,
            BorrowedUntil = until,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = now
        };

        tool.Status = ToolStatus.Lent;
        await loans.AddAsync(loan, ct);

        var created = await loans.FindByIdAsync(loan.Id, ct)
                      ?? throw new InvalidOperationException("Created loan could not be reloaded.");
        return ToDto(created);
    }

    public async Task<LoanDto> UpdateAsync(int borrowerId, int loanId, UpdateLoanRequest request, CancellationToken ct)
    {
        var loan = await RequireOwnEditableLoanAsync(borrowerId, loanId, ct);

        if (request.BorrowedUntil is { } until)
        {
            if (until <= loan.BorrowedFrom)
                throw DomainValidationException.ForField("borrowedUntil", "Borrowed until must be after borrowed from.");

            if (!loan.Tool.IsWithinWindow(loan.BorrowedFrom, until))
            {
                throw DomainValidationException.ForField(
                    "borrowedUntil", "The loan period must fall within the tool's availability window.");
            }

            loan.BorrowedUntil = until;
        }

        if (request.Note is not null)
            loan.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await loans.SaveChangesAsync(ct);
        return ToDto(loan);
    }

    public async Task DeleteAsync(int borrowerId, int loanId, CancellationToken ct)
    {
        var loan = await RequireOwnEditableLoanAsync(borrowerId, loanId, ct);

        // Cancelling an active loan frees the tool; loan.Tool is the same tracked instance
        if (loan.IsActive)
            loan.Tool.Status = ToolStatus.Available;

        loans.Remove(loan);
        await loans.SaveChangesAsync(ct);
    }

    public async Task<LoanDto> ReturnAsync(int ownerId, int toolId, CancellationToken ct)
    {
        var tool = await tools.FindTrackedByIdAsync(toolId, ct)
                   ?? throw new NotFoundException("Tool not found.");

        if (tool.OwnerId != ownerId)
            throw new ForbiddenException("Only the tool's owner can mark it as returned.");

        var loan = await loans.FindActiveByToolIdAsync(toolId, ct)
                   ?? throw new ConflictException("This tool is not currently lent out.");

        loan.ReturnedAt = time.GetUtcNow().UtcDateTime;
        tool.Status = ToolStatus.Available; // same tracked instance as loan.Tool

        await loans.SaveChangesAsync(ct);
        return ToDto(loan);
    }

    private async Task<Loan> RequireOwnEditableLoanAsync(int borrowerId, int loanId, CancellationToken ct)
    {
        var loan = await loans.FindByIdAsync(loanId, ct);

        // Hide existence of loans that aren't the caller's own
        if (loan is null || loan.BorrowerId != borrowerId)
            throw new NotFoundException("Loan not found.");

        if (!loan.IsActive)
            throw new ConflictException("This loan has already been returned.");

        if (!loan.IsWithinEditWindow(time.GetUtcNow().UtcDateTime))
            throw new ConflictException("Loans can only be edited or deleted within 24 hours of creation.");

        return loan;
    }

    private static LoanDto ToDto(Loan l) => new(
        l.Id, l.ToolId, l.Tool.Name, l.BorrowerId, l.Borrower.DisplayName,
        l.BorrowedFrom, l.BorrowedUntil, l.ReturnedAt, l.Note, l.CreatedAt, l.IsActive);
}
