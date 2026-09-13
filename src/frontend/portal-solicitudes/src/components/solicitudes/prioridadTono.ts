import type { Tono } from '../ui/tono';

const TONO_POR_NIVEL: Record<number, Tono> = {
  1: 'exito',
  2: 'advertencia',
  3: 'acento',
  4: 'critico',
};

/** Un nivel fuera del catálogo (0-4) cae a neutral en vez de romper la vista. */
export function obtenerTonoPrioridad(nivel: number): Tono {
  return TONO_POR_NIVEL[nivel] ?? 'neutral';
}
