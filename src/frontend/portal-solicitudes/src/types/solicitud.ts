import type { Area } from './area';
import type { EstadoSolicitud } from './estadoSolicitud';
import type { Prioridad } from './prioridad';
import type { TipoSolicitud } from './tipoSolicitud';
import type { UsuarioResumen } from './usuario';

/**
 * Forma exacta de SolicitudResumenDto (ver ADR-0030): fila de listado y dashboard, siempre con
 * objetos anidados, nunca ids planos — el DTO real no los trae. `descripcion` no viaja aqui,
 * solo en SolicitudDetalle.
 */
export interface Solicitud {
  id: number;
  /** Codigo legible y unico, formato SOL-2026-0001. */
  codigo: string;
  titulo: string;
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
}
