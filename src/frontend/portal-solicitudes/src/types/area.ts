/** Forma exacta de GET /api/areas: la API ya filtra por activas, no manda el campo. */
export interface Area {
  id: number;
  nombre: string;
}

/** Forma de GET /api/areas/todas (Administrador): incluye inactivas. */
export interface AreaAdmin extends Area {
  activo: boolean;
}
