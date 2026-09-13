import type { Solicitud } from './solicitud';

/** Espejo de ConteoEstadoDto. */
export interface ConteoEstado {
  codigoEstado: string;
  nombreEstado: string;
  cantidad: number;
}

/** Espejo de ConteoPrioridadDto. */
export interface ConteoPrioridad {
  prioridadId: number;
  nombrePrioridad: string;
  cantidad: number;
}

/** Espejo de ResumenDashboardDto — GET /api/dashboard/resumen. */
export interface ResumenDashboard {
  porEstado: ConteoEstado[];
  porPrioridad: ConteoPrioridad[];
  totalVencidas: number;
  totalSolicitudes: number;
  recientes: Solicitud[];
  /** Sin responsable, dentro del alcance del usuario — ajeno al `asignacion` activo (ver ADR-0027). */
  totalSinAsignar: number;
}
