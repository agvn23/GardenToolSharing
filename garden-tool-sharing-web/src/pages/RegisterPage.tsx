import { useState, type CSSProperties, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { errorMessage } from '../utils/problemDetails';

export function RegisterPage() {
  const { register, user, isLoading } = useAuth();
  const navigate = useNavigate();

  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);

    if (password !== confirm) {
      setError("The passwords don't match.");
      return;
    }

    setIsSubmitting(true);
    try {
      await register({ displayName: displayName.trim(), email: email.trim(), password });
      navigate('/tools', { replace: true });
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  // Wait for the stored-token check, and send anyone already logged in onward
  if (isLoading) return null;
  if (user) return <Navigate to="/tools" replace />;

  return (
    <div style={styles.page}>
      <form onSubmit={handleSubmit} style={styles.card}>
        <h1 style={styles.heading}>Create an account</h1>

        <label style={styles.label}>
          Display name
          <input
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            required
            minLength={2}
            maxLength={50}
            autoComplete="name"
            style={styles.input}
          />
        </label>

        <label style={styles.label}>
          Email
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            maxLength={254}
            autoComplete="email"
            style={styles.input}
          />
        </label>

        <label style={styles.label}>
          Password (at least 8 characters)
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={8}
            maxLength={100}
            autoComplete="new-password"
            style={styles.input}
          />
        </label>

        <label style={styles.label}>
          Confirm password
          <input
            type="password"
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            required
            autoComplete="new-password"
            style={styles.input}
          />
        </label>

        {error && <p role="alert" style={styles.error}>{error}</p>}

        <button type="submit" disabled={isSubmitting} style={styles.button}>
          {isSubmitting ? 'Creating account…' : 'Create account'}
        </button>

        <p style={styles.footer}>
          Already have an account? <Link to="/login">Log in</Link>
        </p>
      </form>
    </div>
  );
}

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
  footer: { margin: 0, fontSize: '0.875rem' },
};