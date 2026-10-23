import { apiFetch } from '../utils/api';

export interface NoticeComment {
  id: number;
  authorId: number;
  authorName: string;
  body: string;
  createdAt: string;
}

export interface Notice {
  id: number;
  title: string;
  body: string;
  authorId: number;
  authorName: string;
  createdAt: string;
  commentCount: number;
  comments: NoticeComment[] | null; // only filled in on the single-notice view
}

export function getNotices(): Promise<Notice[]> {
  return apiFetch<Notice[]>('/notices');
}

export function getNotice(id: number): Promise<Notice> {
  return apiFetch<Notice>(`/notices/${id}`);
}

export function createNotice(input: { title: string; body: string }): Promise<Notice> {
  return apiFetch<Notice>('/notices', { method: 'POST', body: input });
}

export function deleteNotice(id: number): Promise<void> {
  return apiFetch<void>(`/notices/${id}`, { method: 'DELETE' });
}

export function addComment(noticeId: number, body: string): Promise<NoticeComment> {
  return apiFetch<NoticeComment>(`/notices/${noticeId}/comments`, { method: 'POST', body: { body } });
}

export function deleteComment(noticeId: number, commentId: number): Promise<void> {
  return apiFetch<void>(`/notices/${noticeId}/comments/${commentId}`, { method: 'DELETE' });
}