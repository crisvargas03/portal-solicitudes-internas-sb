/** Forma exacta de GET /api/tipos-solicitud: sin descripcion ni activo en la respuesta. */
export interface TipoSolicitud {
  id: number;
  nombre: string;
}

/** Forma de GET /api/tipos-solicitud/todos (Administrador): incluye inactivos. */
export interface TipoSolicitudAdmin extends TipoSolicitud {
  descripcion: string | null;
  activo: boolean;
}
