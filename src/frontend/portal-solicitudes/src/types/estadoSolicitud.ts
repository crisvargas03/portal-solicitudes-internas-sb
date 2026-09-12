export interface EstadoSolicitud {
  id: number;
  nombre: string;
  activo: boolean;
  /** Clave estable, independiente del id, ver CodigosEstadoSolicitud. */
  codigo: string;
  orden: number;
  esFinal: boolean;
}
