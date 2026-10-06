import { apiFetch } from '../utils/api'; // adjust the path to match your other modules

export type ToolVisibility = 'Public' | 'Private';
export type ToolStatus = 'Available' | 'Lent';

export interface Tool {
  id: number;
  name: string;
  description: string | null;
  visibility: ToolVisibility;
  status: ToolStatus;
  availableFrom: string;  // 'YYYY-MM-DD' (DateOnly arrives as a plain string)
  availableUntil: string;
  ownerId: number;
  ownerName: string;
  createdAt: string;      // ISO date-time
  borrowerName: string | null;
}

export function getTools(options: { mine?: boolean } = {}): Promise<Tool[]> {
  return apiFetch<Tool[]>(`/tools${options.mine ? '?mine=true' : ''}`);
}