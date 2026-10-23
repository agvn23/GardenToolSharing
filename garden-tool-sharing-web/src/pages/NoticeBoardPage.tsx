import { useEffect, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { createNotice, getNotices, type Notice } from '../data/notices';
import { errorMessage } from '../utils/problemDetails';

export function NoticeBoardPage() {
  const [notices, setNotices] = useState<Notice[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;
    getNotices()
      .then((data) => { if (!cancelled) setNotices(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, [reloadKey]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setFormError(null);
    setIsSubmitting(true);
    try {
      await createNotice({ title: title.trim(), body: body.trim() });
      setTitle('');
      setBody('');
      setReloadKey((k) => k + 1);
    } catch (err) {
      setFormError(errorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section>
      <h1>Noticeboard</h1>

      <form
        onSubmit={handleSubmit}
        style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', maxWidth: '500px', marginBottom: '1.5rem' }}
      >
        <h2 style={{ fontSize: '1rem', margin: 0 }}>Post a notice</h2>
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Title"
          required
          minLength={2}
          maxLength={100}
        />
        <textarea
          value={body}
          onChange={(e) => setBody(e.target.value)}
          placeholder="What would you like to say?"
          required
          maxLength={2000}
          rows={4}
        />
        {formError && <p role="alert" style={{ color: '#b91c1c', margin: 0 }}>{formError}</p>}
        <button type="submit" disabled={isSubmitting || !title.trim() || !body.trim()}>
          {isSubmitting ? 'Posting…' : 'Post notice'}
        </button>
      </form>

      {error && <p role="alert">{error}</p>}
      {!error && !notices && <p>Loading notices…</p>}
      {!error && notices && notices.length === 0 && <p>No notices yet. Be the first to post one.</p>}
      {!error && notices && notices.length > 0 && (
        <ul style={{ listStyle: 'none', padding: 0 }}>
          {notices.map((notice) => (
            <li key={notice.id} style={{ marginBottom: '1.25rem', maxWidth: '600px' }}>
              <h2 style={{ fontSize: '1.1rem', margin: '0 0 0.25rem' }}>
                <Link to={`/notices/${notice.id}`}>{notice.title}</Link>
              </h2>
              <div style={{ fontSize: '0.8rem', color: '#555' }}>
                {notice.authorName} · {new Date(notice.createdAt).toLocaleString()}
              </div>
              <p style={{ whiteSpace: 'pre-wrap', margin: '0.5rem 0' }}>{notice.body}</p>
              <Link to={`/notices/${notice.id}`} style={{ fontSize: '0.85rem' }}>
                {notice.commentCount === 0
                  ? 'Add a comment'
                  : `${notice.commentCount} comment${notice.commentCount === 1 ? '' : 's'}`}
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}