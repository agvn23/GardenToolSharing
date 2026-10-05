import { useState, type CSSProperties, type FormEvent } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ApiError } from '../utils/problemDetails';

export function LoginPage() {
  const { login, user, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname: string } } | null)?.from?.pathname ?? '/';

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await login({ email, password });
      navigate(from, { replace: true });
    } catch (err) {
      // 401 from the API carries a plain detail message ("Invalid email or password");
      // anything else (network failure, 500) falls back to a generic message.
      setError(err instanceof ApiError ? err.message : 'Something went wrong. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  }

if (isLoading) return null;
if (user) return <Navigate to={from} replace />;

  return (
    <div style={styles.page}>
      <form onSubmit={handleSubmit} style={styles.card}>
        <h1 style={styles.heading}>Log in</h1>

        <label style={styles.label}>
          Email
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            style={styles.input}
          />
        </label>

        <label style={styles.label}>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
            style={styles.input}
          />
        </label>

        {error && <p style={styles.error}>{error}</p>}

        <button type="submit" disabled={isSubmitting} style={styles.button}>
          {isSubmitting ? 'Logging in…' : 'Log in'}
        </button>
      </form>
    </div>
  );
}

// Plain inline styles for now - a real design pass (palette, type, layout) comes once we're
// building pages with actual content to design around, rather than guessing ahead of it.
const styles: Record<string, CSSProperties> = {
  page: {
    minHeight: '100vh',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontFamily: 'system-ui, sans-serif',
    background: '#f6f5f2',
  },
  card: {
    display: 'flex',
    flexDirection: 'column',
    gap: '0.75rem',
    width: '320px',
    padding: '2rem',
    background: '#fff',
    borderRadius: '8px',
    boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
  },
  heading: { margin: 0, fontSize: '1.5rem' },
  label: { display: 'flex', flexDirection: 'column', gap: '0.25rem', fontSize: '0.875rem' },
  input: { padding: '0.5rem', fontSize: '1rem', border: '1px solid #ccc', borderRadius: '4px' },
  error: { color: '#b91c1c', fontSize: '0.875rem', margin: 0 },
  button: {
    marginTop: '0.5rem',
    padding: '0.6rem',
    fontSize: '1rem',
    background: '#2d6a4f',
    color: '#fff',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
  },
};
