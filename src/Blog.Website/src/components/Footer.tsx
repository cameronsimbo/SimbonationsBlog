export default function Footer() {
  return (
    <footer className="border-t border-gray-200 bg-gray-50 py-8">
      <div className="container-blog text-center text-sm text-gray-500">
        <p>&copy; {new Date().getFullYear()} Simbonations Blog. All rights reserved.</p>
      </div>
    </footer>
  );
}
