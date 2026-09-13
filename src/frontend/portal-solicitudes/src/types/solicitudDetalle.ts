import type { Area } from './area';
import type { EstadoSolicitud } from './estadoSolicitud';
import type { Prioridad } from './prioridad';
import type { TipoSolicitud } from './tipoSolicitud';
import type { UsuarioResumen } from './usuario';

/**
 * Forma exacta de GET /api/solicitudes/{id} (SolicitudDetalleDto): a diferencia de `Solicitud`
 * (usado por la lista y el mock), aca no hay *Id planos, solo los objetos anidados.
 */
export interface SolicitudDetalle {
  id: number;
  /** Codigo legible y unico, formato SOL-2026-0001. */
  codigo: string;
  titulo: string;
  descripcion: string;
  estado: EstadoSolicitud;
  prioridad: Prioridad;
  area: Area;
  tipoSolicitud: TipoSolicitud;
  solicitante: UsuarioResumen;
  asignado: UsuarioResumen | null;
  fechaCreacion: string;
  fechaCompromiso: string | null;
  /** Calculado en el servidor a partir de fechaCompromiso y estado — nunca recalcular en el cliente. */
  estaVencida: boolean;
  /** Reutiliza HistorialEstado.comentario de la transicion a Resuelta (ver ADR-0001). */
  comentarioResolucion: string | null;
  historial: HistorialEstadoDetalle[];
  comentarios: ComentarioDetalle[];
  adjuntos: AdjuntoDetalle[];
}

export interface HistorialEstadoDetalle {
  id: number;
  estadoAnterior: EstadoSolicitud | null;
  estadoNuevo: EstadoSolicitud;
  usuario: UsuarioResumen;
  comentario: string | null;
  fecha: string;
}

/**
 * Forma exacta de ComentarioDto: sin solicitudId/usuarioId (a diferencia de `Comentario`
 * en comentario.ts, que modela la entidad completa pero no lo que la API realmente manda).
 */
export interface ComentarioDetalle {
  id: number;
  texto: string;
  /** true oculta el comentario al solicitante — nunca llega en true para ese rol. */
  esInterno: boolean;
  usuario: UsuarioResumen;
  fecha: string;
}

/** Forma exacta de AdjuntoDto: sin solicitudId/usuarioId, ver nota de ComentarioDetalle. */
export interface AdjuntoDetalle {
  id: number;
  descripcion: string;
  url: string;
  usuario: UsuarioResumen;
  fecha: string;
}
