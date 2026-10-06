import { apiFetch } from '../utils/api';

export interface LeaderboardEntry {
  rank: number;
  ownerId: number;
  ownerName: string;
  totalLoans: number;
}

export function getLeaderboard(period?: string): Promise<LeaderboardEntry[]> {
  const query = period ? `?period=${encodeURIComponent(period)}` : '';
  return apiFetch<LeaderboardEntry[]>(`/leaderboard${query}`);
}