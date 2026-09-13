import { LogOut, UserCircle } from 'lucide-react';
import { useNavigate } from 'react-router';
import { usePageTitle } from '../../hooks/usePageTitle';
import { useAuthStore } from '../../store/authStore';

export function Header() {
  const title = usePageTitle();
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
  }

  return (
    <header className="flex h-16 shrink-0 items-center justify-between border-b border-slate-200 bg-white px-8">
      <h1 className="text-lg font-semibold text-navy">{title}</h1>
      <div className="flex items-center gap-4 text-sm text-slate-600">
        <div className="flex items-center gap-2">
          <UserCircle size={20} className="text-accent-orange" />
          {user?.nombre ?? 'Invitado'}
        </div>
        <button
          type="button"
          onClick={handleLogout}
          className="flex items-center gap-1.5 text-slate-500 transition-colors hover:text-accent-orange"
          title="Cerrar sesión"
        >
          <LogOut size={16} />
          Cerrar sesión
        </button>
      </div>
    </header>
  );
}
