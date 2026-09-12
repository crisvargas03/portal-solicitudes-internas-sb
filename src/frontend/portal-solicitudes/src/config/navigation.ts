import { ClipboardList, FilePlus2, LayoutDashboard, type LucideIcon } from 'lucide-react';

export interface NavItemConfig {
  to: string;
  label: string;
  icon: LucideIcon;
}

export const NAV_ITEMS: NavItemConfig[] = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/solicitudes', label: 'Solicitudes', icon: ClipboardList },
  { to: '/solicitudes/nueva', label: 'Nueva solicitud', icon: FilePlus2 },
];
