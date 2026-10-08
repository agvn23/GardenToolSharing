import { useNavigate } from 'react-router-dom';
import { ToolForm } from '../components/ToolForm';
import { createTool } from '../data/tools';

export function NewToolPage() {
  const navigate = useNavigate();

  return (
    <section>
      <h1>Add a tool</h1>
      <ToolForm
        submitLabel="Add tool"
        onSubmit={async (input) => {
          await createTool(input);
          navigate('/tools');
        }}
      />
    </section>
  );
}