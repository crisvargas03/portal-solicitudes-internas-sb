import { ClipboardList, FilePlus2, FolderCog, Landmark, LayoutDashboard, Users, type LucideIcon } from 'lucide-react';
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
  { to: '/solicitudes/nueva', label: 'Nueva solicitud', icon: FilePlus2 },
  { to: '/catalogos', label: 'Catálogos', icon: FolderCog, rolesPermitidos: ['Administrador'] },
  {
    to: '/entidades-gubernamentales',
    label: 'Entidades gubernamentales',
    icon: Landmark,
    rolesPermitidos: ['Administrador'],
  },
  { to: '/usuarios', label: 'Usuarios', icon: Users, rolesPermitidos: ['Administrador'] },
];
