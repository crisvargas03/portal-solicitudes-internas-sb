import { LogOut, UserCircle } from 'lucide-react';
import { NavLink, useNavigate } from 'react-router';
import logo from '../../assets/logo-superintendencia-de-bancos.png';
import { NAV_ITEMS } from '../../config/navigation';
import { useConfirm } from '../../hooks/useConfirm';
import { useRolActual } from '../../hooks/useRolActual';
import { useAuthStore } from '../../store/authStore';

export function Sidebar() {
  const { rol } = useRolActual();
  const items = NAV_ITEMS.filter((item) => !item.rolesPermitidos || (rol && item.rolesPermitidos.includes(rol)));
  const navigate = useNavigate();
  const confirmar = useConfirm();
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  async function handleLogout() {
    const confirmado = await confirmar({
      titulo: 'Cerrar sesión',
      mensaje: '¿Cerrar tu sesión en el portal? Se perderá cualquier cambio que no hayas guardado.',
      textoConfirmar: 'Cerrar sesión',
    });
    if (!confirmado) return;
    logout();
    navigate('/login', { replace: true });
  }

  return (
    <aside className="flex w-60 shrink-0 flex-col bg-navy">
      <div className="flex items-center px-6 py-6">
        <img src={logo} alt="Superintendencia de Bancos" className="h-10 w-auto" />
      </div>
      <nav className="flex flex-col gap-1 px-3">
        {items.map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            end
            className={({ isActive }) =>
              `flex items-center gap-3 rounded-r-md border-l-2 py-2.5 pl-4 pr-3 text-sm font-medium transition-colors ${
                isActive
                  ? 'border-accent-orange text-accent-orange'
                  : 'border-transparent text-white/70 hover:border-white/20 hover:text-white'
              }`
            }
          >
            <Icon size={18} strokeWidth={2} />
            {label}
          </NavLink>
        ))}
      </nav>

      <div className="mt-auto border-t border-white/10 px-3 py-4">
        <div className="flex items-center gap-3 px-4 py-2 text-sm text-white/70">
          <UserCircle size={20} className="text-accent-orange" />
          <span className="truncate">{user?.nombre ?? 'Invitado'}</span>
        </div>
        <button
          type="button"
          onClick={() => void handleLogout()}
          className="flex w-full items-center gap-3 rounded-r-md border-l-2 border-transparent py-2.5 pl-4 pr-3 text-sm font-medium text-white/70 transition-colors hover:border-white/20 hover:text-white"
        >
          <LogOut size={18} strokeWidth={2} />
          Cerrar sesión
        </button>
      </div>
    </aside>
  );
}
