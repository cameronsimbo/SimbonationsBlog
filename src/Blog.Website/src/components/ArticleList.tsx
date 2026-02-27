import type { ArticleSummary } from '@/types';
import ArticleCard from './ArticleCard';

interface ArticleListProps {
  articles: ArticleSummary[];
}

export default function ArticleList({ articles }: ArticleListProps) {
  if (articles.length === 0) {
    return (
      <div className="py-12 text-center text-gray-500">
        <p>No articles found.</p>
      </div>
    );
  }

  return (
    <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
      {articles.map((article) => (
        <ArticleCard key={article.id} article={article} />
      ))}
    </div>
  );
}
