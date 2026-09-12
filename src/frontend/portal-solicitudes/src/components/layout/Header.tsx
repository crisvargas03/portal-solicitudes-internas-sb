import { UserCircle } from 'lucide-react';
import { usePageTitle } from '../../hooks/usePageTitle';
import { useAuthStore } from '../../store/authStore';

export function Header() {
  const title = usePageTitle();
  const user = useAuthStore((state) => state.user);

  return (
    <header className="flex h-16 shrink-0 items-center justify-between border-b border-slate-200 bg-white px-8">
      <h1 className="text-lg font-semibold text-navy">{title}</h1>
      <div className="flex items-center gap-2 text-sm text-slate-600">
        <UserCircle size={20} className="text-accent-orange" />
        {user?.nombre ?? 'Invitado'}
      </div>
    </header>
  );
}
