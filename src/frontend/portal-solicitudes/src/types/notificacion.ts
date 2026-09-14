import type { Usuario } from './usuario';

export type CanalNotificacion = 'Consola' | 'CorreoSimulado' | 'BaseDeDatos';

export type EstadoNotificacion = 'Pendiente' | 'Enviada' | 'Fallida';

export interface Notificacion {
  id: number;
  solicitudId: number;
  usuarioDestinoId: number;
  usuarioDestino?: Usuario;
  canal: CanalNotificacion;
  asunto: string;
  mensaje: string;
  estado: EstadoNotificacion;
  fecha: string;
}
