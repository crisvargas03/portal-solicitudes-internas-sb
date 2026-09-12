import type { Usuario } from './usuario';

export interface Comentario {
  id: number;
  solicitudId: number;
  usuarioId: number;
  usuario?: Usuario;
  texto: string;
  /** true oculta el comentario al solicitante. */
  esInterno: boolean;
  fecha: string;
}
