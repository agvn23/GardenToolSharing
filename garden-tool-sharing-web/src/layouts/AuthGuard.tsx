import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

/** Wraps private routes. Redirects to /login (remembering where the user was headed) if
 * there's no authenticated user once the initial session check has finished. */
export function AuthGuard() {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <p style={{ padding: '2rem', fontFamily: 'system-ui, sans-serif' }}>Loading…</p>;
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}
