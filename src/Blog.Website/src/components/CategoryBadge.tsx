interface CategoryBadgeProps {
  name: string;
}

export default function CategoryBadge({ name }: CategoryBadgeProps) {
  return (
    <span className="inline-flex items-center rounded-full bg-primary-50 px-2.5 py-0.5 text-xs font-medium text-primary-700">
      {name}
    </span>
  );
}
