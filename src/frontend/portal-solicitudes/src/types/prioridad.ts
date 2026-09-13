/** Forma exacta de GET /api/prioridades: la API ya filtra por activas, no manda el campo. */
export interface Prioridad {
  id: number;
  nombre: string;
  /** Severidad relativa: a mayor numero, mayor urgencia. */
  nivel: number;
}

/** Forma de GET /api/prioridades/todas (Administrador): incluye inactivas. */
export interface PrioridadAdmin extends Prioridad {
  activo: boolean;
}
