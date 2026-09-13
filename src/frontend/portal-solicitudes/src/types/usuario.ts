import type { RolUsuario } from './rolUsuario';

export interface Usuario {
  id: number;
  nombre: string;
  email: string;
  rol: RolUsuario;
  /** GET /api/auth/login y GET /api/auth/me no lo mandan; solo lo trae GET /api/usuarios. */
  activo?: boolean;
}
