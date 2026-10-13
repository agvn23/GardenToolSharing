import type { ToolPhoto } from '../data/tools';
import { AuthImage } from './AuthImage';

interface Props {
  toolId: number;
  photos: ToolPhoto[];
  onDelete?: (photo: ToolPhoto) => void;
}

export function PhotoGallery({ toolId, photos, onDelete }: Props) {
  return (
    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem', margin: '0.5rem 0' }}>
      {photos.map((photo) => (
        <figure key={photo.id} style={{ margin: 0, maxWidth: '100%' }}>
          <AuthImage toolId={toolId} photoId={photo.id} alt={photo.caption} />
          <figcaption style={{ fontSize: '0.8rem' }}>
            <strong>{photo.caption}</strong>
            <br />
            Uploaded {new Date(photo.createdAt).toLocaleDateString()}
            {onDelete && (
              <>
                {' '}
                <button type="button" onClick={() => onDelete(photo)}>Delete</button>
              </>
            )}
          </figcaption>
        </figure>
      ))}
    </div>
  );
}