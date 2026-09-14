import { usePageTitle } from '../../hooks/usePageTitle';

export function Header() {
  const title = usePageTitle();

  return (
    <header className="flex h-16 shrink-0 items-center border-l border-white/10 bg-navy px-8">
      <h1 className="text-lg font-semibold text-white">{title}</h1>
    </header>
  );
}
