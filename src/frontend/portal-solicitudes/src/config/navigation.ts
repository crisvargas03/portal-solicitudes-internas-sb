import { ClipboardList, FilePlus2, LayoutDashboard, type LucideIcon } from 'lucide-react';
import type { RolUsuario } from '../types';

export interface NavItemConfig {
  to: string;
  label: string;
  icon: LucideIcon;
  /** Si se omite, el ítem es visible para cualquier rol. */
  rolesPermitidos?: RolUsuario[];
}

export const NAV_ITEMS: NavItemConfig[] = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/solicitudes', label: 'Solicitudes', icon: ClipboardList },
  { to: '/solicitudes/nueva', label: 'Nueva solicitud', icon: FilePlus2, rolesPermitidos: ['Administrador', 'Solicitante'] },
];
