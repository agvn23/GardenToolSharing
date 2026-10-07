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
  myMembershipStatus: 'Pending' | 'Active' | null;
  myLoan: MyLoan | null;
}

export function getTools(options: { mine?: boolean } = {}): Promise<Tool[]> {
  return apiFetch<Tool[]>(`/tools${options.mine ? '?mine=true' : ''}`);
}

export interface CreateToolInput {
  name: string;
  description?: string;
  visibility: ToolVisibility;
  availableFrom: string;  // 'YYYY-MM-DD'
  availableUntil: string;
}

export function createTool(input: CreateToolInput): Promise<Tool> {
  return apiFetch<Tool>('/tools', { method: 'POST', body: input });
}

export interface MyLoan {
  id: number;
  borrowedFrom: string;
  borrowedUntil: string;
  note: string | null;
  editableUntil: string; // ISO date-time (UTC)
}