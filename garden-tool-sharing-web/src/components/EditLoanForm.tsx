import { useState, type FormEvent } from 'react';
import { updateLoan } from '../data/loans';
import type { MyLoan, Tool } from '../data/tools';
import { errorMessage } from '../utils/problemDetails';

interface Props {
  tool: Tool;
  loan: MyLoan;
  onDone: () => void;
  onCancel: () => void;
}

function dayAfter(date: string): string {
  const d = new Date(`${date}T00:00:00`);
  d.setDate(d.getDate() + 1);
  return d.toLocaleDateString('en-CA');
}

export function EditLoanForm({ tool, loan, onDone, onCancel }: Props) {
  const [borrowedUntil, setBorrowedUntil] = useState(loan.borrowedUntil);
  const [note, setNote] = useState(loan.note ?? '');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await updateLoan(loan.id, { borrowedUntil, note });
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
        Borrowed until
        <input
          type="date"
          value={borrowedUntil}
          min={dayAfter(loan.borrowedFrom)}
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
        <button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</button>
        <button type="button" onClick={onCancel} disabled={isSubmitting}>Cancel</button>
      </div>
    </form>
  );
}