import { useEffect, useState } from 'react';
import { approveMembership, getPendingRequests, type Membership } from '../data/memberships';
import { errorMessage } from '../utils/problemDetails';

export function RequestsPage() {
  const [requests, setRequests] = useState<Membership[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<number | null>(null);

  useEffect(() => {
    let cancelled = false;
    getPendingRequests()
      .then((data) => { if (!cancelled) setRequests(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, []);

  async function handleApprove(request: Membership) {
    setActionError(null);
    setBusyId(request.id);
    try {
      await approveMembership(request.id);
      setRequests((current) => current?.filter((r) => r.id !== request.id) ?? null);
    } catch (err) {
      setActionError(errorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <section>
      <h1>Requests</h1>
      <p style={{ color: '#555' }}>People waiting for your approval to borrow your private tools.</p>

      {error && <p role="alert">{error}</p>}
      {actionError && <p role="alert" style={{ color: '#b91c1c' }}>{actionError}</p>}
      {!error && !requests && <p>Loading requests…</p>}
      {!error && requests && requests.length === 0 && <p>No pending requests.</p>}
      {!error && requests && requests.length > 0 && (
        <ul>
          {requests.map((request) => (
            <li key={request.id} style={{ marginBottom: '0.75rem' }}>
              <strong>{request.userDisplayName}</strong> wants to borrow <strong>{request.toolName}</strong>
              <br />
              Requested {new Date(request.createdAt).toLocaleDateString()}{' '}
              <button onClick={() => handleApprove(request)} disabled={busyId === request.id}>
                {busyId === request.id ? 'Approving…' : 'Approve'}
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}