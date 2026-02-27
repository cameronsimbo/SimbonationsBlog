import Link from 'next/link';
import { getCategories } from '@/lib/api';
import type { Category } from '@/types';

export default async function CategoriesPage() {
  let categories: Category[] = [];
  try {
    categories = await getCategories();
  } catch {
    // API not available
  }

  return (
    <div className="container-blog py-12">
      <h1 className="mb-8">Categories</h1>
      {categories.length === 0 ? (
        <p className="text-gray-500">No categories found.</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {categories.map((category) => (
            <Link
              key={category.id}
              href={`/articles?category=${category.id}`}
              className="rounded-lg border border-gray-200 p-6 transition-shadow hover:shadow-md"
            >
              <h2 className="text-lg font-semibold text-gray-900">{category.name}</h2>
              {category.description && (
                <p className="mt-2 text-sm text-gray-600">{category.description}</p>
              )}
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
