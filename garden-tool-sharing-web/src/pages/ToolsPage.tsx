import { useEffect, useState } from 'react';
import { getTools, type Tool } from '../data/tools'; // adjust the path

export function ToolsPage() {
  const [tools, setTools] = useState<Tool[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getTools()
      .then((data) => { if (!cancelled) setTools(data); })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Could not load tools.');
      });
    return () => { cancelled = true; };
  }, []);

  if (error) return <p role="alert">{error}</p>;
  if (!tools) return <p>Loading tools…</p>;
  if (tools.length === 0) return <p>No tools to show yet.</p>;

  return (
    <section>
      <h1>Tools</h1>
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
    </section>
  );
}