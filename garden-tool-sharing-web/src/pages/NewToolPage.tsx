import { useNavigate } from 'react-router-dom';
import { ToolForm } from '../components/ToolForm';
import { createTool } from '../data/tools';
import { uploadQueuedPhotos } from '../utils/photos';
import { errorMessage } from '../utils/problemDetails';

export function NewToolPage() {
  const navigate = useNavigate();

  return (
    <section>
      <h1>Add a tool</h1>
      <ToolForm
        submitLabel="Add tool"
        onSubmit={async (input, photos) => {
          const tool = await createTool(input);
          try {
            await uploadQueuedPhotos(tool.id, photos);
          } catch (err) {
            navigate(`/tools/${tool.id}/edit`, {
              state: { notice: `Tool added, but a photo didn't upload: ${errorMessage(err)}` },
            });
            return;
          }
          navigate('/tools');
        }}
      />
    </section>
  );
}