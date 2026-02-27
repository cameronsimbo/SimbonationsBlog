'use client';

import { useState } from 'react';
import type { Comment } from '@/types';
import { createComment } from '@/lib/api';
import { formatDate } from '@/lib/utils';

interface CommentSectionProps {
  articleId: string;
  comments: Comment[];
}

export default function CommentSection({ articleId, comments }: CommentSectionProps) {
  const [authorName, setAuthorName] = useState('');
  const [authorEmail, setAuthorEmail] = useState('');
  const [content, setContent] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setMessage('');

    try {
      await createComment({
        articleId,
        authorName,
        authorEmail,
        content,
      });
      setAuthorName('');
      setAuthorEmail('');
      setContent('');
      setMessage('Comment submitted! It will appear after moderation.');
    } catch {
      setMessage('Failed to submit comment. Please try again.');
    } finally {
      setSubmitting(false);
    }
  }

  function renderComment(comment: Comment, depth = 0) {
    return (
      <div
        key={comment.id}
        className={`border-l-2 border-gray-200 pl-4 ${depth > 0 ? 'ml-4 mt-3' : 'mt-4'}`}
      >
        <div className="flex items-center gap-2 text-sm">
          <span className="font-medium text-gray-900">{comment.authorName}</span>
          <span className="text-gray-400">&middot;</span>
          <time className="text-gray-400">{formatDate(comment.createdDate)}</time>
        </div>
        <p className="mt-1 text-sm text-gray-700">{comment.content}</p>
        {comment.replies?.map((reply) => renderComment(reply, depth + 1))}
      </div>
    );
  }

  return (
    <section className="mt-12">
      <h3 className="text-lg font-semibold">Comments ({comments.length})</h3>

      <div className="mt-4 space-y-0">
        {comments.map((comment) => renderComment(comment))}
      </div>

      {comments.length === 0 && (
        <p className="mt-4 text-sm text-gray-500">No comments yet. Be the first!</p>
      )}

      <form onSubmit={handleSubmit} className="mt-8 space-y-4 rounded-lg border border-gray-200 p-6">
        <h4 className="font-medium">Leave a comment</h4>
        <div className="grid gap-4 sm:grid-cols-2">
          <input
            type="text"
            placeholder="Your name"
            value={authorName}
            onChange={(e) => setAuthorName(e.target.value)}
            required
            className="input-field rounded-md border border-gray-300 px-3 py-2"
          />
          <input
            type="email"
            placeholder="Your email"
            value={authorEmail}
            onChange={(e) => setAuthorEmail(e.target.value)}
            required
            className="input-field rounded-md border border-gray-300 px-3 py-2"
          />
        </div>
        <textarea
          placeholder="Write your comment..."
          value={content}
          onChange={(e) => setContent(e.target.value)}
          required
          rows={4}
          className="input-field w-full rounded-md border border-gray-300 px-3 py-2"
        />
        <div className="flex items-center gap-4">
          <button type="submit" disabled={submitting} className="btn-primary">
            {submitting ? 'Submitting...' : 'Post Comment'}
          </button>
          {message && <p className="text-sm text-green-600">{message}</p>}
        </div>
      </form>
    </section>
  );
}
