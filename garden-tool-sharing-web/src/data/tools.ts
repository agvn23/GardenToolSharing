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

export function getTool(id: number): Promise<Tool> {
  return apiFetch<Tool>(`/tools/${id}`);
}

export function updateTool(id: number, input: CreateToolInput): Promise<Tool> {
  return apiFetch<Tool>(`/tools/${id}`, { method: 'PUT', body: input });
}

export function removeTool(id: number): Promise<void> {
  return apiFetch<void>(`/tools/${id}`, { method: 'DELETE' });
}

export function restoreTool(id: number): Promise<Tool> {
  return apiFetch<Tool>(`/tools/${id}:restore`, { method: 'POST' });
}

export function getHiddenTools(): Promise<Tool[]> {
  return apiFetch<Tool[]>('/tools/hidden');
}

export interface MyLoan {
  id: number;
  borrowedFrom: string;
  borrowedUntil: string;
  note: string | null;
  editableUntil: string; // ISO date-time (UTC)
}