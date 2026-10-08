import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getHiddenTools, restoreTool, type Tool } from '../data/tools';
import { errorMessage } from '../utils/problemDetails';

export function HiddenToolsPage() {
  const [tools, setTools] = useState<Tool[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<number | null>(null);

  useEffect(() => {
    let cancelled = false;
    getHiddenTools()
      .then((data) => { if (!cancelled) setTools(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, []);

  async function handleRestore(tool: Tool) {
    setActionError(null);
    setBusyId(tool.id);
    try {
      await restoreTool(tool.id);
      setTools((current) => current?.filter((t) => t.id !== tool.id) ?? null);
    } catch (err) {
      setActionError(errorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <section>
      <h1>Hidden tools</h1>
      <p style={{ color: '#555' }}>
        Tools you've removed. Nobody else can see or borrow them until you restore them.
      </p>

      {error && <p role="alert">{error}</p>}
      {actionError && <p role="alert" style={{ color: '#b91c1c' }}>{actionError}</p>}
      {!error && !tools && <p>Loading…</p>}
      {!error && tools && tools.length === 0 && <p>You have no hidden tools.</p>}
      {!error && tools && tools.length > 0 && (
        <ul>
          {tools.map((tool) => (
            <li key={tool.id} style={{ marginBottom: '1rem' }}>
              <strong>{tool.name}</strong>
              <br />
              Available {tool.availableFrom} to {tool.availableUntil}
              {tool.description && <p style={{ margin: '0.25rem 0' }}>{tool.description}</p>}
              <button onClick={() => handleRestore(tool)} disabled={busyId === tool.id}>
                {busyId === tool.id ? 'Restoring…' : 'Restore'}
              </button>
            </li>
          ))}
        </ul>
      )}

      <p><Link to="/tools">Back to tools</Link></p>
    </section>
  );
}