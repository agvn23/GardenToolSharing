import { useEffect, useState } from 'react';
import { apiFetchBlob } from '../utils/api';

interface Props {
  toolId: number;
  photoId: number;
  alt: string;
}

const box = {
  height: 96,
  width: 128,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  background: '#eee',
  color: '#666',
  fontSize: '0.8rem',
  borderRadius: 4,
} as const;

export function AuthImage({ toolId, photoId, alt }: Props) {
  const [url, setUrl] = useState<string | null>(null);
  const [failed, setFailed] = useState(false);
  const [expanded, setExpanded] = useState(false);

  useEffect(() => {
    let objectUrl: string | null = null;
    let cancelled = false;
    apiFetchBlob(`/tools/${toolId}/photos/${photoId}`)
      .then((blob) => {
        if (cancelled) return;
        objectUrl = URL.createObjectURL(blob);
        setUrl(objectUrl);
      })
      .catch(() => { if (!cancelled) setFailed(true); });
    return () => {
      cancelled = true;
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [toolId, photoId]);

  if (failed) return <div style={box}>Photo unavailable</div>;
  if (!url) return <div style={box}>Loading…</div>;

  return (
    <img
      src={url}
      alt={alt}
      onClick={() => setExpanded((e) => !e)}
      style={{
        display: 'block',
        borderRadius: 4,
        cursor: expanded ? 'zoom-out' : 'zoom-in',
        ...(expanded ? { width: 'min(100%, 480px)', height: 'auto' } : { height: 96, width: 'auto' }),
      }}
    />
  );
}