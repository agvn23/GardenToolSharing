import { useState, type FormEvent } from 'react';
import { createLoan, joinTool } from '../data/loans';
import type { Tool } from '../data/tools';
import { ApiError, errorMessage } from '../utils/problemDetails';

interface Props {
  tool: Tool;
  onDone: () => void;
  onCancel: () => void;
}

export function BorrowForm({ tool, onDone, onCancel }: Props) {
  const today = new Date().toLocaleDateString('en-CA');
  // The loan has to start inside the owner's availability window
  const earliest = today > tool.availableFrom ? today : tool.availableFrom;

  const [borrowedFrom, setBorrowedFrom] = useState(earliest);
  const [borrowedUntil, setBorrowedUntil] = useState('');
  const [note, setNote] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      try {
        await joinTool(tool.id); // public tools approve instantly; 409 means already a member
      } catch (err) {
        if (!(err instanceof ApiError && err.status === 409)) throw err;
      }
      await createLoan({
        toolId: tool.id,
        borrowedFrom,
        borrowedUntil,
        note: note.trim() || undefined,
      });
      onDone();
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', maxWidth: '320px', marginTop: '0.5rem' }}
    >
      <label>
        From
        <input
          type="date"
          value={borrowedFrom}
          min={earliest}
          max={tool.availableUntil}
          onChange={(e) => setBorrowedFrom(e.target.value)}
          required
          style={{ display: 'block' }}
        />
      </label>
      <label>
        Until
        <input
          type="date"
          value={borrowedUntil}
          min={borrowedFrom}
          max={tool.availableUntil}
          onChange={(e) => setBorrowedUntil(e.target.value)}
          required
          style={{ display: 'block' }}
        />
      </label>
      <label>
        Note (optional)
        <input
          value={note}
          onChange={(e) => setNote(e.target.value)}
          maxLength={500}
          style={{ display: 'block', width: '100%' }}
        />
      </label>
      {error && <p role="alert" style={{ color: '#b91c1c', margin: 0 }}>{error}</p>}
      <div style={{ display: 'flex', gap: '0.5rem' }}>
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving…' : 'Confirm borrow'}
        </button>
        <button type="button" onClick={onCancel} disabled={isSubmitting}>Cancel</button>
      </div>
    </form>
  );
}