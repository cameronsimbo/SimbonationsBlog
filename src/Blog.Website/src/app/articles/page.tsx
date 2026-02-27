import { getArticles } from '@/lib/api';
import ArticleList from '@/components/ArticleList';
import Pagination from '@/components/Pagination';

interface ArticlesPageProps {
  searchParams: Promise<{ page?: string; search?: string }>;
}

export default async function ArticlesPage({ searchParams }: ArticlesPageProps) {
  const params = await searchParams;
  const page = Number(params.page) || 1;
  const searchTerm = params.search || undefined;

  let articles;
  try {
    articles = await getArticles({ pageNumber: page, pageSize: 9, searchTerm });
  } catch {
    articles = { items: [], pageNumber: 1, pageSize: 9, totalCount: 0, totalPages: 0, hasPreviousPage: false, hasNextPage: false };
  }

  return (
    <div className="container-blog py-12">
      <h1 className="mb-8">Articles</h1>
      <ArticleList articles={articles.items} />
      <Pagination
        currentPage={articles.pageNumber}
        totalPages={articles.totalPages}
        basePath="/articles"
      />
    </div>
  );
}
