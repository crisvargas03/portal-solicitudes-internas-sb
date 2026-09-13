import type { EstadoSolicitud } from '../../types';
import { Badge } from '../ui/Badge';
import { obtenerTonoEstado } from './estadoTono';

interface StatusBadgeProps {
  estado?: EstadoSolicitud;
}

export function StatusBadge({ estado }: StatusBadgeProps) {
  if (!estado) return <Badge tono="neutral">Sin estado</Badge>;
  return <Badge tono={obtenerTonoEstado(estado.codigo)}>{estado.nombre}</Badge>;
}
