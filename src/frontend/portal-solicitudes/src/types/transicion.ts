/** Espejo de TransicionDisponibleDto — GET /api/solicitudes/{id}/transiciones. */
export interface TransicionDisponible {
  estadoDestinoId: number;
  /** Ver CodigosEstadoSolicitud. */
  codigo: string;
  nombre: string;
  requiereComentario: boolean;
}
