export interface Prioridad {
  id: number;
  nombre: string;
  activo: boolean;
  /** Severidad relativa: a mayor numero, mayor urgencia. */
  nivel: number;
}
