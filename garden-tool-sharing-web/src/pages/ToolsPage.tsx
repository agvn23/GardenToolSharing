import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { BorrowForm } from '../components/BorrowForm';
import { useAuth } from '../contexts/AuthContext';
import { getTools, type Tool } from '../data/tools';
import { returnTool } from '../data/loans';
import { errorMessage } from '../utils/problemDetails';
import { EditLoanForm } from '../components/EditLoanForm';
import { cancelLoan } from '../data/loans';
import { removeTool } from '../data/tools';

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

  async function handleRemove(tool: Tool) {
    if (!window.confirm(`Remove ${tool.name}? It will be hidden from everyone until you restore it from Hidden tools.`)) return;
    setActionError(null);
    try {
        await removeTool(tool.id);
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

  const [editingId, setEditingId] = useState<number | null>(null);

function canEditLoan(tool: Tool): boolean {
  return !!tool.myLoan && new Date(tool.myLoan.editableUntil) > new Date();
}

async function handleCancelLoan(tool: Tool) {
  if (!tool.myLoan) return;
  if (!window.confirm(`Cancel your loan of ${tool.name}?`)) return;
  setActionError(null);
  try {
    await cancelLoan(tool.myLoan.id);
    setReloadKey((k) => k + 1);
  } catch (err) {
    setActionError(errorMessage(err));
  }
}

  return (
    <section>
      <h1>Tools</h1>
      <p><Link to="/tools/new">Add a tool</Link> · <Link to="/tools/hidden">Hidden tools</Link></p>

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

                {tool.myLoan && (
                    <p style={{ margin: '0.25rem 0' }}>
                        Your loan: {tool.myLoan.borrowedFrom} to {tool.myLoan.borrowedUntil}
                        {tool.myLoan.note && ` ("${tool.myLoan.note}")`}
                    </p>
                )}

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

                {isOwner && tool.status === 'Available' && (
                    <>
                        <Link to={`/tools/${tool.id}/edit`}>Edit</Link>{' '}
                        <button onClick={() => handleRemove(tool)}>Remove</button>
                    </>
                )}

                {canEditLoan(tool) && editingId !== tool.id && (
                    <>
                        <button onClick={() => { setActionError(null); setEditingId(tool.id); }}>Edit loan</button>{' '}
                        <button onClick={() => handleCancelLoan(tool)}>Cancel loan</button>
                    </>
                )}
                {editingId === tool.id && tool.myLoan && (
                    <EditLoanForm
                        tool={tool}
                        loan={tool.myLoan}
                        onDone={() => { setEditingId(null); setReloadKey((k) => k + 1); }}
                        onCancel={() => setEditingId(null)}
                    />
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