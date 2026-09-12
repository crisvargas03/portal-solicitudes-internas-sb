import type { Area } from './area';
import type { EstadoSolicitud } from './estadoSolicitud';
import type { Prioridad } from './prioridad';
import type { TipoSolicitud } from './tipoSolicitud';
import type { Usuario } from './usuario';

export interface Solicitud {
  id: number;
  /** Codigo legible y unico, formato SOL-2026-0001. */
  codigo: string;
  titulo: string;
  descripcion: string;
  fechaCreacion: string;
  fechaCompromiso?: string;
  prioridadId: number;
  prioridad?: Prioridad;
  estadoId: number;
  estado?: EstadoSolicitud;
  areaId: number;
  area?: Area;
  tipoSolicitudId: number;
  tipoSolicitud?: TipoSolicitud;
  usuarioSolicitanteId: number;
  usuarioSolicitante?: Usuario;
  usuarioAsignadoId?: number;
  usuarioAsignado?: Usuario;
}
