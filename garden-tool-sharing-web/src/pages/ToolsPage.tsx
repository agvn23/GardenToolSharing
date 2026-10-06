import { useEffect, useState } from 'react';
import { getTools, type Tool } from '../data/tools'; // keep the path you already use

export function ToolsPage() {
  const [mine, setMine] = useState(false);
  const [tools, setTools] = useState<Tool[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getTools({ mine })
      .then((data) => { if (!cancelled) setTools(data); })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Could not load tools.');
      });
    return () => { cancelled = true; };
  }, [mine]);

  function handleToggle(checked: boolean) {
    setTools(null); // show "Loading" while the new list loads
    setError(null);
    setMine(checked);
  }

  return (
    <section>
      <h1>Tools</h1>

      <label style={{ display: 'inline-flex', gap: '0.5rem', alignItems: 'center' }}>
        <input
          type="checkbox"
          checked={mine}
          onChange={(e) => handleToggle(e.target.checked)}
        />
        Show only my tools (owned or borrowed)
      </label>

      {error && <p role="alert">{error}</p>}
      {!error && !tools && <p>Loading tools…</p>}
      {!error && tools && tools.length === 0 && (
        <p>{mine ? "You haven't added any tools yet." : 'No tools to show yet.'}</p>
      )}
      {!error && tools && tools.length > 0 && (
        <ul>
          {tools.map((tool) => (
            <li key={tool.id}>
              <strong>{tool.name}</strong> ({tool.status}) owned by {tool.ownerName}
              <br />
              Available {tool.availableFrom} to {tool.availableUntil}
              {tool.description && <p>{tool.description}</p>}
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}