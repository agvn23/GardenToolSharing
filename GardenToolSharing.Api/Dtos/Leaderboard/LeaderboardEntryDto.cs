namespace GardenToolSharing.Api.Dtos.Leaderboard;

public record LeaderboardEntryDto(int Rank, int OwnerId, string OwnerName, int TotalLoans, int ActiveLoans);
