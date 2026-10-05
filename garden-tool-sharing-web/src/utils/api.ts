import { getStoredToken } from './authStorage';
import { parseApiError } from './problemDetails';

const baseUrl = import.meta.env.VITE_API_BASE_URL;

interface ApiRequestOptions extends Omit<RequestInit, 'body'> {
  body?: unknown; // JSON.stringified automatically if present
}

/** Fetch wrapper used by every actions/ and data/ module: resolves against VITE_API_BASE_URL,
 * attaches the stored bearer token, JSON-encodes the body, and throws an ApiError on failure. */
export async function apiFetch<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const token = getStoredToken();
  const headers = new Headers(options.headers);
  headers.set('Accept', 'application/json');
  if (options.body !== undefined) headers.set('Content-Type', 'application/json');
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const response = await fetch(`${baseUrl}${path}`, {
    ...options,
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  if (!response.ok) {
    throw await parseApiError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
