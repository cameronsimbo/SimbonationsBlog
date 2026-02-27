import Link from 'next/link';
import type { ArticleSummary } from '@/types';
import { formatDate } from '@/lib/utils';
import CategoryBadge from './CategoryBadge';

interface ArticleCardProps {
  article: ArticleSummary;
}

export default function ArticleCard({ article }: ArticleCardProps) {
  return (
    <article className="group rounded-lg border border-gray-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
      {article.coverImageUrl && (
        <div className="mb-4 overflow-hidden rounded-md">
          <img
            src={article.coverImageUrl}
            alt={article.title}
            className="h-48 w-full object-cover transition-transform group-hover:scale-105"
          />
        </div>
      )}
      <div className="flex items-center gap-2 text-xs text-gray-500">
        <CategoryBadge name={article.categoryName} />
        <span>&middot;</span>
        <time dateTime={article.publishedDate || undefined}>
          {formatDate(article.publishedDate)}
        </time>
      </div>
      <Link href={`/articles/${article.slug}`}>
        <h2 className="mt-2 text-xl font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
          {article.title}
        </h2>
      </Link>
      <p className="mt-2 text-sm text-gray-600 line-clamp-3">{article.excerpt}</p>
      <div className="mt-4 text-xs text-gray-400">
        By {article.authorName}
      </div>
    </article>
  );
}
