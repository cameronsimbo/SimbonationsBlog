import { getArticles } from '@/lib/api';
import ArticleList from '@/components/ArticleList';
import Pagination from '@/components/Pagination';

interface HomePageProps {
  searchParams: Promise<{ page?: string }>;
}

export default async function HomePage({ searchParams }: HomePageProps) {
  const params = await searchParams;
  const page = Number(params.page) || 1;

  let articles;
  try {
    articles = await getArticles({ pageNumber: page, pageSize: 6 });
  } catch {
    articles = { items: [], pageNumber: 1, pageSize: 6, totalCount: 0, totalPages: 0, hasPreviousPage: false, hasNextPage: false };
  }

  return (
    <div className="container-blog py-12">
      <section className="mb-12 text-center">
        <h1 className="text-5xl font-bold tracking-tight text-gray-900">
          Simbonations Blog
        </h1>
        <p className="mt-4 text-lg text-gray-600">
          Thoughts, stories, and ideas about software development.
        </p>
      </section>

      <ArticleList articles={articles.items} />

      <Pagination
        currentPage={articles.pageNumber}
        totalPages={articles.totalPages}
        basePath="/"
      />
    </div>
  );
}
