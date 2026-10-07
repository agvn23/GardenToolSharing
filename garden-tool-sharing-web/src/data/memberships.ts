import { apiFetch } from '../utils/api';

export interface Membership {
  id: number;
  toolId: number;
  toolName: string;
  userId: number;
  userDisplayName: string;
  status: 'Pending' | 'Active';
  createdAt: string;
}

export function getPendingRequests(): Promise<Membership[]> {
  return apiFetch<Membership[]>('/memberships/pending');
}

export function approveMembership(id: number): Promise<Membership> {
  return apiFetch<Membership>(`/memberships/${id}`, { method: 'PATCH' });
}

export function requestAccess(toolId: number): Promise<Membership> {
  return apiFetch<Membership>('/memberships', { method: 'POST', body: { toolId } });
}