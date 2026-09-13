import type { RolUsuario } from './rolUsuario';

export interface Usuario {
  id: number;
  nombre: string;
  email: string;
  rol: RolUsuario;
  activo: boolean;
}

/** Forma exacta de GET /api/usuarios. */
export interface UsuarioResumen {
  id: number;
  nombre: string;
  email: string;
  rol: RolUsuario;
  activo: boolean;
}
