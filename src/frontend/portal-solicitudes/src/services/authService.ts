import { get, post } from '../lib/apiClient';
import type { Sesion } from '../store/authStore';
import type { Usuario } from '../types';

export interface CredencialesLogin {
  email: string;
  password: string;
}

/** POST /api/auth/login. datos: { token, expiraEn, usuario }. */
export async function iniciarSesion(credenciales: CredencialesLogin): Promise<Sesion> {
  const respuesta = await post<{ token: string; expiraEn: string; usuario: Usuario }>(
    '/auth/login',
    credenciales,
  );

  return { usuario: respuesta.usuario, token: respuesta.token, expiraEn: respuesta.expiraEn };
}

/** GET /api/auth/me — usado para revalidar un token persistido al arrancar la app. */
export async function obtenerUsuarioActual(): Promise<Usuario> {
  return get<Usuario>('/auth/me');
}

/**
 * POST /api/auth/refresh — re-emision deslizante (ver ADR-0019): exige el token todavia
 * vigente en el interceptor, no manda cuerpo. El usuario sale del token, no de aca.
 */
export async function renovarSesion(): Promise<Sesion> {
  const respuesta = await post<{ token: string; expiraEn: string; usuario: Usuario }>('/auth/refresh');

  return { usuario: respuesta.usuario, token: respuesta.token, expiraEn: respuesta.expiraEn };
}
