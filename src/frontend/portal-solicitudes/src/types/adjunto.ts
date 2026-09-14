import type { Usuario } from './usuario';

export interface Adjunto {
  id: number;
  solicitudId: number;
  usuarioId: number;
  usuario?: Usuario;
  descripcion: string;
  url: string;
  fecha: string;
}
