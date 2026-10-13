import type { ChangeEvent } from 'react';
import type { QueuedPhoto } from '../utils/photos';

interface Props {
  photos: QueuedPhoto[];
  onChange: (next: QueuedPhoto[]) => void;
  max: number;
}

export function PhotoFields({ photos, onChange, max }: Props) {
  const room = max - photos.length;

  function handleFiles(event: ChangeEvent<HTMLInputElement>) {
    const files = Array.from(event.target.files ?? []).slice(0, Math.max(room, 0));
    const added = files.map((file, i) => ({
      id: crypto.randomUUID(),
      file,
      caption: photos.length + i === 0 ? 'Overview' : photos.length + i === 1 ? 'Existing damage' : '',
    }));
    onChange([...photos, ...added]);
    event.target.value = ''; // lets the same file be picked again
  }

  return (
    <fieldset style={{ border: '1px solid #ccc', borderRadius: 4 }}>
      <legend>Photos</legend>

      <datalist id="photo-captions">
        <option value="Overview" />
        <option value="Existing damage" />
      </datalist>

      {photos.map((photo) => (
        <div key={photo.id} style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', marginBottom: '0.5rem' }}>
          <span style={{ fontSize: '0.85rem', flex: '1 1 0', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            {photo.file.name}
          </span>
          <input
            list="photo-captions"
            value={photo.caption}
            maxLength={100}
            placeholder="Caption"
            onChange={(e) =>
              onChange(photos.map((p) => (p.id === photo.id ? { ...p, caption: e.target.value } : p)))
            }
          />
          <button type="button" onClick={() => onChange(photos.filter((p) => p.id !== photo.id))}>
            Remove
          </button>
        </div>
      ))}

      {room > 0 ? (
        <label>
          Add photos (up to {room} more)
          <input type="file" accept="image/*" multiple onChange={handleFiles} style={{ display: 'block' }} />
        </label>
      ) : (
        <p style={{ margin: 0 }}>This tool has the maximum number of photos.</p>
      )}
    </fieldset>
  );
}