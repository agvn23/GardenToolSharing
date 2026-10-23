import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { addComment, deleteComment, deleteNotice, getNotice, type Notice, type NoticeComment } from '../data/notices';
import { errorMessage } from '../utils/problemDetails';

export function NoticePage() {
  const { id } = useParams();
  const noticeId = Number(id);
  const validId = Number.isInteger(noticeId) && noticeId > 0;
  const { user } = useAuth();
  const navigate = useNavigate();

  const [notice, setNotice] = useState<Notice | null>(null);
  const [error, setError] = useState<string | null>(validId ? null : "That link isn't valid.");
  const [actionError, setActionError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);
  const [comment, setComment] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!validId) return;
    let cancelled = false;
    getNotice(noticeId)
      .then((data) => { if (!cancelled) setNotice(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, [noticeId, validId, reloadKey]);

  async function handleComment(event: FormEvent) {
    event.preventDefault();
    setActionError(null);
    setIsSubmitting(true);
    try {
      await addComment(noticeId, comment.trim());
      setComment('');
      setReloadKey((k) => k + 1);
    } catch (err) {
      setActionError(errorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeleteNotice() {
    if (!window.confirm('Delete this notice and all its comments?')) return;
    setActionError(null);
    try {
      await deleteNotice(noticeId);
      navigate('/notices');
    } catch (err) {
      setActionError(errorMessage(err));
    }
  }

  async function handleDeleteComment(target: NoticeComment) {
    if (!window.confirm('Delete this comment?')) return;
    setActionError(null);
    try {
      await deleteComment(noticeId, target.id);
      setReloadKey((k) => k + 1);
    } catch (err) {
      setActionError(errorMessage(err));
    }
  }

  if (error) {
    return (
      <section>
        <p role="alert">{error}</p>
        <p><Link to="/notices">Back to the noticeboard</Link></p>
      </section>
    );
  }
  if (!notice) return <p>Loading notice…</p>;

  const isAuthor = user?.id === notice.authorId;
  const comments = notice.comments ?? [];

  return (
    <section style={{ maxWidth: '600px' }}>
      <p><Link to="/notices">← Noticeboard</Link></p>

      <h1 style={{ marginBottom: '0.25rem' }}>{notice.title}</h1>
      <div style={{ fontSize: '0.8rem', color: '#555' }}>
        {notice.authorName} · {new Date(notice.createdAt).toLocaleString()}
      </div>
      <p style={{ whiteSpace: 'pre-wrap' }}>{notice.body}</p>
      {isAuthor && <button onClick={handleDeleteNotice}>Delete notice</button>}

      <h2 style={{ fontSize: '1rem', marginTop: '1.5rem' }}>
        {comments.length === 0 ? 'No comments yet' : `Comments (${comments.length})`}
      </h2>

      {actionError && <p role="alert" style={{ color: '#b91c1c' }}>{actionError}</p>}

      <ul style={{ listStyle: 'none', padding: 0 }}>
        {comments.map((c) => (
          <li key={c.id} style={{ marginBottom: '0.75rem' }}>
            <div style={{ fontSize: '0.8rem', color: '#555' }}>
              <strong>{c.authorName}</strong> · {new Date(c.createdAt).toLocaleString()}
              {(user?.id === c.authorId || isAuthor) && (
                <>
                  {' '}
                  <button type="button" onClick={() => handleDeleteComment(c)}>Delete</button>
                </>
              )}
            </div>
            <div style={{ whiteSpace: 'pre-wrap' }}>{c.body}</div>
          </li>
        ))}
      </ul>

      <form onSubmit={handleComment} style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
        <textarea
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Write a comment"
          required
          maxLength={1000}
          rows={3}
        />
        <button type="submit" disabled={isSubmitting || !comment.trim()}>
          {isSubmitting ? 'Posting…' : 'Add comment'}
        </button>
      </form>
    </section>
  );
}