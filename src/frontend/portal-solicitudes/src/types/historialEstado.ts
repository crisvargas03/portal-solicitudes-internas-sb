import type { EstadoSolicitud } from './estadoSolicitud';
import type { Usuario } from './usuario';

export interface HistorialEstado {
  id: number;
  solicitudId: number;
  estadoAnteriorId?: number;
  estadoAnterior?: EstadoSolicitud;
  estadoNuevoId: number;
  estadoNuevo?: EstadoSolicitud;
  usuarioId: number;
  usuario?: Usuario;
  comentario?: string;
  fecha: string;
}
