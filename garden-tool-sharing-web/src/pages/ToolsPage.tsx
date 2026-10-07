import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { BorrowForm } from '../components/BorrowForm';
import { useAuth } from '../contexts/AuthContext';
import { getTools, type Tool } from '../data/tools';
import { returnTool } from '../data/loans';
import { errorMessage } from '../utils/problemDetails';

export function ToolsPage() {
  const { user } = useAuth();
  const [mine, setMine] = useState(false);
  const [tools, setTools] = useState<Tool[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);
  const [borrowingId, setBorrowingId] = useState<number | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getTools({ mine })
      .then((data) => { if (!cancelled) setTools(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, [mine, reloadKey]);

  function handleToggle(checked: boolean) {
    setTools(null);
    setError(null);
    setBorrowingId(null);
    setMine(checked);
  }

  function statusLabel(tool: Tool): string {
    if (tool.status === 'Lent') {
        return tool.borrowerName ? `Borrowed by ${tool.borrowerName}` : 'Borrowed';
    }
    if (tool.myMembershipStatus === 'Pending') return 'Waiting for approval';
    return 'Available';
    }

  async function handleReturn(tool: Tool) {
    if (!window.confirm(`Mark ${tool.name} as returned?`)) return;
    setActionError(null);
    try {
      await returnTool(tool.id);
      setReloadKey((k) => k + 1);
    } catch (err) {
      setActionError(errorMessage(err));
    }
  }

  async function shareRequestLink(tool: Tool) {
    const link = `${window.location.origin}/tools/${tool.id}/request`;
    try {
        await navigator.clipboard.writeText(link);
        window.alert(`Request link copied:\n${link}`);
    } catch {
        window.prompt('Copy this request link:', link);
    }
  }

  return (
    <section>
      <h1>Tools</h1>
      <p><Link to="/tools/new">Add a tool</Link></p>

      <label style={{ display: 'inline-flex', gap: '0.5rem', alignItems: 'center' }}>
        <input type="checkbox" checked={mine} onChange={(e) => handleToggle(e.target.checked)} />
        Show only my tools (owned or borrowed)
      </label>

      {error && <p role="alert">{error}</p>}
      {actionError && <p role="alert" style={{ color: '#b91c1c' }}>{actionError}</p>}
      {!error && !tools && <p>Loading tools…</p>}
      {!error && tools && tools.length === 0 && (
        <p>{mine ? "You don't own or borrow any tools yet." : 'No tools to show yet.'}</p>
      )}
      {!error && tools && tools.length > 0 && (
        <ul>
          {tools.map((tool) => {
            const isOwner = user?.id === tool.ownerId;
            return (
            <li key={tool.id} style={{ marginBottom: '1rem' }}>
                <strong>{tool.name}</strong> ({statusLabel(tool)}) owned by {tool.ownerName}
                <br />
                Available {tool.availableFrom} to {tool.availableUntil}
                {tool.description && <p style={{ margin: '0.25rem 0' }}>{tool.description}</p>}

                {tool.status === 'Available' && !isOwner && tool.myMembershipStatus !== 'Pending' && borrowingId !== tool.id && (
                    <button onClick={() => { setActionError(null); setBorrowingId(tool.id); }}>
                        Borrow
                    </button>
                )}
                {tool.status === 'Lent' && isOwner && (
                    <button onClick={() => handleReturn(tool)}>Mark as returned</button>
                )}
                {isOwner && tool.visibility === 'Private' && (
                    <button onClick={() => shareRequestLink(tool)}>Copy request link</button>
                )}
                {borrowingId === tool.id && (
                    <BorrowForm
                        tool={tool}
                        onDone={() => { setBorrowingId(null); setReloadKey((k) => k + 1); }}
                        onCancel={() => setBorrowingId(null)}
                    />
                )}
            </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}