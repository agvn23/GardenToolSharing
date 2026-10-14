import { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';
import { PhotoGallery } from '../components/PhotoGallery';
import { ToolForm } from '../components/ToolForm';
import { deleteToolPhoto, getTool, updateTool, type Tool, type ToolPhoto } from '../data/tools';
import { uploadQueuedPhotos } from '../utils/photos';
import { errorMessage } from '../utils/problemDetails';

export function EditToolPage() {
  const { id } = useParams();
  const toolId = Number(id);
  const validId = Number.isInteger(toolId) && toolId > 0;
  const navigate = useNavigate();
  const location = useLocation();
  const notice = (location.state as { notice?: string } | null)?.notice ?? null;

  const [tool, setTool] = useState<Tool | null>(null);
  const [error, setError] = useState<string | null>(validId ? null : "That link isn't valid.");
  const [photoError, setPhotoError] = useState<string | null>(null);

  useEffect(() => {
    if (!validId) return;
    let cancelled = false;
    getTool(toolId)
      .then((data) => { if (!cancelled) setTool(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, [toolId, validId]);

  async function handleDeletePhoto(photo: ToolPhoto) {
    if (!tool) return;
    if (!window.confirm(`Delete the photo "${photo.caption}"?`)) return;
    setPhotoError(null);
    try {
      await deleteToolPhoto(tool.id, photo.id);
      setTool({ ...tool, photos: (tool.photos ?? []).filter((p) => p.id !== photo.id) });
    } catch (err) {
      setPhotoError(errorMessage(err));
    }
  }

  if (error) {
    return (
      <section>
        <p role="alert">{error}</p>
        <p><Link to="/tools">Back to tools</Link></p>
      </section>
    );
  }
  if (!tool) return <p>Loading tool…</p>;

  const existing = tool.photos ?? [];

  return (
    <section>
      <h1>Edit {tool.name}</h1>
      {notice && <p role="alert" style={{ color: '#b91c1c' }}>{notice}</p>}

      {existing.length > 0 && (
        <>
          <h2 style={{ fontSize: '1rem' }}>Current photos</h2>
          <PhotoGallery toolId={tool.id} photos={existing} onDelete={handleDeletePhoto} />
          {photoError && <p role="alert" style={{ color: '#b91c1c' }}>{photoError}</p>}
        </>
      )}

      <ToolForm
        initial={tool}
        submitLabel="Save changes"
        maxNewPhotos={6 - existing.length}
        onSubmit={async (input, photos) => {
          await updateTool(tool.id, input);
          try {
            await uploadQueuedPhotos(tool.id, photos);
          } catch (err) {
            throw new Error(
                `Details saved, but a photo didn't upload: ${errorMessage(err)}. Reload the page to see which photos made it.`,
                { cause: err },
            );
          navigate('/tools');
        }}
    }/>
      <p><Link to="/tools">Cancel</Link></p>
    </section>
  );
}