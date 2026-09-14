import type { Prioridad } from '../../types';
import { Badge } from '../ui/Badge';
import { obtenerTonoPrioridad } from './prioridadTono';

interface PriorityBadgeProps {
  prioridad?: Prioridad;
}

export function PriorityBadge({ prioridad }: PriorityBadgeProps) {
  if (!prioridad) return <Badge tono="neutral">Sin prioridad</Badge>;
  // Crítica es la única severidad que debe leerse como una interrupción, no como un dato más.
  const variante = prioridad.nivel >= 4 ? 'solido' : 'suave';
  return (
    <Badge tono={obtenerTonoPrioridad(prioridad.nivel)} variante={variante}>
      {prioridad.nombre}
    </Badge>
  );
}
