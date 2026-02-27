import { fetchApi } from './utils';
import type {
  Article,
  ArticleSummary,
  Author,
  Category,
  Comment,
  PaginatedList,
  Tag,
} from '@/types';

// Articles
export function getArticles(params?: {
  pageNumber?: number;
  pageSize?: number;
  categoryId?: string;
  tagId?: string;
  searchTerm?: string;
}): Promise<PaginatedList<ArticleSummary>> {
  const searchParams = new URLSearchParams();
  if (params?.pageNumber) searchParams.set('pageNumber', String(params.pageNumber));
  if (params?.pageSize) searchParams.set('pageSize', String(params.pageSize));
  if (params?.categoryId) searchParams.set('categoryId', params.categoryId);
  if (params?.tagId) searchParams.set('tagId', params.tagId);
  if (params?.searchTerm) searchParams.set('searchTerm', params.searchTerm);

  const query = searchParams.toString();
  return fetchApi<PaginatedList<ArticleSummary>>(`/articles${query ? `?${query}` : ''}`);
}

export function getArticle(id: string): Promise<Article> {
  return fetchApi<Article>(`/articles/${id}`);
}

export function getArticleBySlug(slug: string): Promise<Article> {
  return fetchApi<Article>(`/articles/by-slug/${slug}`);
}

export function createArticle(data: {
  title: string;
  content: string;
  excerpt: string;
  authorId: string;
  categoryId: string;
  tagIds: string[];
}): Promise<string> {
  return fetchApi<string>('/articles', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export function updateArticle(
  id: string,
  data: {
    title: string;
    content: string;
    excerpt: string;
    categoryId: string;
    tagIds: string[];
  }
): Promise<void> {
  return fetchApi<void>(`/articles/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
}

export function deleteArticle(id: string): Promise<void> {
  return fetchApi<void>(`/articles/${id}`, { method: 'DELETE' });
}

export function publishArticle(id: string): Promise<void> {
  return fetchApi<void>(`/articles/${id}/publish`, { method: 'POST' });
}

// Authors
export function getAuthors(): Promise<Author[]> {
  return fetchApi<Author[]>('/authors');
}

export function getAuthor(id: string): Promise<Author> {
  return fetchApi<Author>(`/authors/${id}`);
}

// Categories
export function getCategories(): Promise<Category[]> {
  return fetchApi<Category[]>('/categories');
}

// Tags
export function getTags(): Promise<Tag[]> {
  return fetchApi<Tag[]>('/tags');
}

// Comments
export function getCommentsByArticle(articleId: string): Promise<Comment[]> {
  return fetchApi<Comment[]>(`/comments/article/${articleId}`);
}

export function createComment(data: {
  articleId: string;
  authorName: string;
  authorEmail: string;
  content: string;
  parentCommentId?: string;
}): Promise<string> {
  return fetchApi<string>('/comments', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}
