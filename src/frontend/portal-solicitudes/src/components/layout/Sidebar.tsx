import { NavLink } from 'react-router';
import logo from '../../assets/logo-superintendencia-de-bancos.png';
import { NAV_ITEMS } from '../../config/navigation';

export function Sidebar() {
  return (
    <aside className="flex w-60 shrink-0 flex-col bg-navy">
      <div className="flex items-center px-6 py-6">
        <img src={logo} alt="Superintendencia de Bancos" className="h-10 w-auto" />
      </div>
      <nav className="flex flex-col gap-1 px-3">
        {NAV_ITEMS.map(({ to, label, icon: Icon }) => (
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
    </aside>
  );
}
