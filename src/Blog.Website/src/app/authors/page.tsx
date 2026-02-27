import { getAuthors } from '@/lib/api';
import type { Author } from '@/types';

export default async function AuthorsPage() {
  let authors: Author[] = [];
  try {
    authors = await getAuthors();
  } catch {
    // API not available
  }

  return (
    <div className="container-blog py-12">
      <h1 className="mb-8">Authors</h1>
      {authors.length === 0 ? (
        <p className="text-gray-500">No authors found.</p>
      ) : (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {authors.map((author) => (
            <div
              key={author.id}
              className="rounded-lg border border-gray-200 p-6"
            >
              <div className="flex items-center gap-4">
                {author.avatarUrl ? (
                  <img
                    src={author.avatarUrl}
                    alt={author.displayName}
                    className="h-12 w-12 rounded-full object-cover"
                  />
                ) : (
                  <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary-100 text-primary-700 font-semibold">
                    {author.displayName.charAt(0)}
                  </div>
                )}
                <div>
                  <h2 className="font-semibold text-gray-900">{author.displayName}</h2>
                  <p className="text-sm text-gray-500">{author.email}</p>
                </div>
              </div>
              {author.bio && (
                <p className="mt-4 text-sm text-gray-600">{author.bio}</p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
