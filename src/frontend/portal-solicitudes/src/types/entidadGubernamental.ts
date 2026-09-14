/** Forma exacta de GET /api/entidades-gubernamentales/todas (Administrador): incluye inactivas. */
export interface EntidadGubernamentalAdmin {
  id: number;
  nombre: string;
  categoria: string;
  poderDelEstado: string;
  sector: string;
  activo: boolean;
}
