import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { apiFetch } from '../utils/api';
import { getStoredToken, setStoredToken, clearStoredToken } from '../utils/authStorage';
import type { User } from '../types/user';
import type { AuthResponse, LoginRequest } from '../types/auth';

interface AuthContextValue {
  user: User | null;
  token: string | null;
  /** True only while the initial stored-token check is in flight. AuthGuard waits on this
   * so it doesn't redirect to /login before a valid stored session has had a chance to load. */
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const stored = getStoredToken();
    if (!stored) {
      setIsLoading(false);
      return;
    }

    // A stored token might be expired or for a user that no longer exists -
    // verify it against the API rather than trusting it blindly.
    apiFetch<User>('/auth/me')
      .then((currentUser) => {
        setToken(stored);
        setUser(currentUser);
      })
      .catch(() => {
        clearStoredToken();
      })
      .finally(() => setIsLoading(false));
  }, []);

  async function login(credentials: LoginRequest) {
    const response = await apiFetch<AuthResponse>('/auth/login', {
      method: 'POST',
      body: credentials,
    });
    setStoredToken(response.token);
    setToken(response.token);
    setUser(response.user);
  }

  function logout() {
    clearStoredToken();
    setToken(null);
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, token, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
