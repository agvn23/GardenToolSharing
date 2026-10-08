import { useState, type FormEvent } from 'react';
import type { CreateToolInput, Tool, ToolVisibility } from '../data/tools';
import { errorMessage } from '../utils/problemDetails';

interface Props {
  initial?: Tool;
  submitLabel: string;
  onSubmit: (input: CreateToolInput) => Promise<void>;
}

export function ToolForm({ initial, submitLabel, onSubmit }: Props) {
  const today = new Date().toLocaleDateString('en-CA'); // local date as YYYY-MM-DD

  const [name, setName] = useState(initial?.name ?? '');
  const [description, setDescription] = useState(initial?.description ?? '');
  const [visibility, setVisibility] = useState<ToolVisibility>(initial?.visibility ?? 'Public');
  const [availableFrom, setAvailableFrom] = useState(initial?.availableFrom ?? today);
  const [availableUntil, setAvailableUntil] = useState(initial?.availableUntil ?? '');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit({
        name: name.trim(),
        description: description.trim() || undefined,
        visibility,
        availableFrom,
        availableUntil,
      });
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', maxWidth: '400px' }}
    >
      <label>
        Name
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          minLength={2}
          maxLength={100}
          style={{ display: 'block', width: '100%' }}
        />
      </label>

      <label>
        Description (optional)
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          maxLength={1000}
          rows={3}
          style={{ display: 'block', width: '100%' }}
        />
      </label>

      <label>
        Who can borrow it?
        <select
          value={visibility}
          onChange={(e) => setVisibility(e.target.value as ToolVisibility)}
          style={{ display: 'block' }}
        >
          <option value="Public">Public: listed for everyone, anyone can borrow</option>
          <option value="Private">Private: hidden, borrowers need your approval</option>
        </select>
      </label>

      <label>
        Available from
        <input
          type="date"
          value={availableFrom}
          onChange={(e) => setAvailableFrom(e.target.value)}
          required
          style={{ display: 'block' }}
        />
      </label>

      <label>
        Available until
        <input
          type="date"
          value={availableUntil}
          min={availableFrom}
          onChange={(e) => setAvailableUntil(e.target.value)}
          required
          style={{ display: 'block' }}
        />
      </label>

      {error && <p role="alert" style={{ color: '#b91c1c', margin: 0 }}>{error}</p>}

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving…' : submitLabel}
      </button>
    </form>
  );
}