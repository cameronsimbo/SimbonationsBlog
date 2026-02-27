import Link from 'next/link';

export default function Header() {
  return (
    <header className="border-b border-gray-200 bg-white">
      <div className="container-blog flex h-16 items-center justify-between">
        <Link href="/" className="text-xl font-bold text-primary-600">
          Simbonations
        </Link>
        <nav className="flex items-center gap-6">
          <Link
            href="/"
            className="text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors"
          >
            Home
          </Link>
          <Link
            href="/articles"
            className="text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors"
          >
            Articles
          </Link>
          <Link
            href="/categories"
            className="text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors"
          >
            Categories
          </Link>
          <Link
            href="/authors"
            className="text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors"
          >
            Authors
          </Link>
          <Link
            href="/admin"
            className="btn-primary text-xs"
          >
            Admin
          </Link>
        </nav>
      </div>
    </header>
  );
}
