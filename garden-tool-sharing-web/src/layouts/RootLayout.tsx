import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export function RootLayout() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
  }

  return (
    <>
      <header
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          padding: '0.75rem 1.5rem',
          background: '#2d6a4f',
          fontFamily: 'system-ui, sans-serif',
        }}
      >
        <nav style={{ display: 'flex', gap: '1rem' }}>
          <Link to="/tools" style={{ color: '#fff', textDecoration: 'none', fontWeight: 600 }}>
            Tools
          </Link>
          <Link to="/leaderboard" style={{ color: '#fff', textDecoration: 'none', fontWeight: 600 }}>
            Leaderboard
          </Link>
          </nav>
        <button
          onClick={handleLogout}
          style={{
            padding: '0.4rem 0.8rem',
            background: '#fff',
            color: '#2d6a4f',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer',
          }}
        >
          Log out
        </button>
      </header>
      <main style={{ padding: '1.5rem', fontFamily: 'system-ui, sans-serif' }}>
        <Outlet />
      </main>
    </>
  );
}