/** Forma exacta de GET /api/estados-solicitud: la API ya filtra por activos, no manda el campo. */
export interface EstadoSolicitud {
  id: number;
  nombre: string;
  /** Clave estable, independiente del id, ver CodigosEstadoSolicitud. */
  codigo: string;
  orden: number;
  esFinal: boolean;
}
