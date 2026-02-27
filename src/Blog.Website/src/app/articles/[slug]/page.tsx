import { notFound } from 'next/navigation';
import { getArticleBySlug, getCommentsByArticle } from '@/lib/api';
import { formatDate } from '@/lib/utils';
import CategoryBadge from '@/components/CategoryBadge';
import TagBadge from '@/components/TagBadge';
import CommentSection from '@/components/CommentSection';
import type { Article, Comment } from '@/types';

interface ArticleDetailPageProps {
  params: Promise<{ slug: string }>;
}

export default async function ArticleDetailPage({ params }: ArticleDetailPageProps) {
  const { slug } = await params;

  let article: Article | undefined;
  try {
    article = await getArticleBySlug(slug);
  } catch {
    notFound();
  }

  if (!article) { notFound(); }

  let comments: Comment[] = [];
  try {
    comments = await getCommentsByArticle(article.id);
  } catch {
    comments = [];
  }

  return (
    <div className="container-blog py-12">
      <article className="mx-auto max-w-3xl">
        {article.coverImageUrl && (
          <img
            src={article.coverImageUrl}
            alt={article.title}
            className="mb-8 h-64 w-full rounded-lg object-cover"
          />
        )}

        <div className="flex items-center gap-3 text-sm text-gray-500">
          <CategoryBadge name={article.categoryName} />
          <time dateTime={article.publishedDate || undefined}>
            {formatDate(article.publishedDate)}
          </time>
          <span>By {article.authorName}</span>
        </div>

        <h1 className="mt-4 text-4xl font-bold tracking-tight text-gray-900">
          {article.title}
        </h1>

        {article.tags.length > 0 && (
          <div className="mt-4 flex flex-wrap gap-2">
            {article.tags.map((tag) => (
              <TagBadge key={tag.id} name={tag.name} />
            ))}
          </div>
        )}

        <div
          className="prose prose-lg mt-8 max-w-none"
          dangerouslySetInnerHTML={{ __html: article.content }}
        />

        <CommentSection articleId={article.id} comments={comments} />
      </article>
    </div>
  );
}
