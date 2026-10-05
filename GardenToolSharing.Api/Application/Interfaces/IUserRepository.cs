using GardenToolSharing.Api.Models;

namespace GardenToolSharing.Api.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct);
    Task<User?> FindByIdAsync(int id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}
