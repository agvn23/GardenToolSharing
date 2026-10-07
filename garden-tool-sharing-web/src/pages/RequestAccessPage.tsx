import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { requestAccess, type Membership } from '../data/memberships';
import { errorMessage } from '../utils/problemDetails';

export function RequestAccessPage() {
  const { id } = useParams();
  const toolId = Number(id);
  const [result, setResult] = useState<Membership | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!Number.isInteger(toolId) || toolId <= 0) {
    return <p role="alert">That request link isn't valid.</p>;
  }

  async function handleRequest() {
    setError(null);
    setIsSubmitting(true);
    try {
      setResult(await requestAccess(toolId));
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section>
      <h1>Request access to a tool</h1>

      {!result && (
        <>
          <p>The owner will need to approve your request before you can borrow it.</p>
          <button onClick={handleRequest} disabled={isSubmitting}>
            {isSubmitting ? 'Sending…' : 'Request access'}
          </button>
        </>
      )}

      {result?.status === 'Pending' && (
        <p>
          Request sent for <strong>{result.toolName}</strong>. It will show as "Waiting for approval" in
          your tools list, and you can borrow it once the owner approves.
        </p>
      )}
      {result?.status === 'Active' && (
        <p>You're approved to borrow <strong>{result.toolName}</strong>.</p>
      )}

      {error && <p role="alert" style={{ color: '#b91c1c' }}>{error}</p>}

      <p><Link to="/tools">Back to tools</Link></p>
    </section>
  );
}