import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { createTool, type ToolVisibility } from '../data/tools';

export function NewToolPage() {
  const navigate = useNavigate();
  const today = new Date().toLocaleDateString('en-CA'); // local date as YYYY-MM-DD

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [visibility, setVisibility] = useState<ToolVisibility>('Public');
  const [availableFrom, setAvailableFrom] = useState(today);
  const [availableUntil, setAvailableUntil] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await createTool({
        name: name.trim(),
        description: description.trim() || undefined,
        visibility,
        availableFrom,
        availableUntil,
      });
      navigate('/tools');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not save the tool. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section>
      <h1>Add a tool</h1>
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
            <option value="Public">Anyone (public)</option>
            <option value="Private">Only people I approve (private)</option>
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
          {isSubmitting ? 'Saving…' : 'Add tool'}
        </button>
      </form>
    </section>
  );
}