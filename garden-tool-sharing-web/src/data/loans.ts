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

export interface UpdateLoanInput {
  borrowedUntil?: string;
  note?: string;
}

export function updateLoan(id: number, input: UpdateLoanInput): Promise<Loan> {
  return apiFetch<Loan>(`/loans/${id}`, { method: 'PATCH', body: input });
}

export function cancelLoan(id: number): Promise<void> {
  return apiFetch<void>(`/loans/${id}`, { method: 'DELETE' });
}