// Plain localStorage wrapper, kept separate from AuthContext so utils/api.ts (a non-React
// module) can read the token without depending on React context. Not in the original FR018
// file list; added because api.ts needs a token source that isn't tied to a component tree.
const TOKEN_KEY = 'gts_token';

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setStoredToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearStoredToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}
