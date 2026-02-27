export interface Article {
  id: string;
  title: string;
  slug: string;
  content: string;
  excerpt: string;
  coverImageUrl: string | null;
  status: ArticleStatus;
  publishedDate: string | null;
  createdDate: string;
  authorId: string;
  authorName: string;
  categoryId: string;
  categoryName: string;
  tags: Tag[];
}

export interface ArticleSummary {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  coverImageUrl: string | null;
  publishedDate: string | null;
  authorName: string;
  categoryName: string;
}

export interface Author {
  id: string;
  displayName: string;
  email: string;
  bio: string;
  avatarUrl: string | null;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string;
}

export interface Tag {
  id: string;
  name: string;
  slug: string;
}

export interface Comment {
  id: string;
  authorName: string;
  content: string;
  status: CommentStatus;
  createdDate: string;
  parentCommentId: string | null;
  replies: Comment[];
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export type ArticleStatus = 'Draft' | 'Published' | 'Archived';
export type CommentStatus = 'Pending' | 'Approved' | 'Rejected';
