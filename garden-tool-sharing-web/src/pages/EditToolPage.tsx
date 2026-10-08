import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ToolForm } from '../components/ToolForm';
import { getTool, updateTool, type Tool } from '../data/tools';
import { errorMessage } from '../utils/problemDetails';

export function EditToolPage() {
  const { id } = useParams();
  const toolId = Number(id);
  const validId = Number.isInteger(toolId) && toolId > 0;
  const navigate = useNavigate();

  const [tool, setTool] = useState<Tool | null>(null);
  const [error, setError] = useState<string | null>(validId ? null : "That link isn't valid.");

  useEffect(() => {
    if (!validId) return;
    let cancelled = false;
    getTool(toolId)
      .then((data) => { if (!cancelled) setTool(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, [toolId, validId]);

  if (error) {
    return (
      <section>
        <p role="alert">{error}</p>
        <p><Link to="/tools">Back to tools</Link></p>
      </section>
    );
  }
  if (!tool) return <p>Loading tool…</p>;

  return (
    <section>
      <h1>Edit {tool.name}</h1>
      <ToolForm
        initial={tool}
        submitLabel="Save changes"
        onSubmit={async (input) => {
          await updateTool(tool.id, input);
          navigate('/tools');
        }}
      />
      <p><Link to="/tools">Cancel</Link></p>
    </section>
  );
}