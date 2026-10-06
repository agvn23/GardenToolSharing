import { apiFetch } from '../utils/api';

export interface Loan {
  id: number;
  toolId: number;
  toolName: string;
  borrowerId: number;
  borrowerName: string;
  borrowedFrom: string;
  borrowedUntil: string;
  returnedAt: string | null;
  note: string | null;
  createdAt: string;
  isActive: boolean;
}

export interface CreateLoanInput {
  toolId: number;
  borrowedFrom: string;   // 'YYYY-MM-DD'
  borrowedUntil: string;
  note?: string;
}

export function joinTool(toolId: number): Promise<unknown> {
  return apiFetch('/memberships', { method: 'POST', body: { toolId } });
}

export function createLoan(input: CreateLoanInput): Promise<Loan> {
  return apiFetch<Loan>('/loans', { method: 'POST', body: input });
}

export function returnTool(toolId: number): Promise<Loan> {
  return apiFetch<Loan>(`/tools/${toolId}:return`, { method: 'POST' });
}