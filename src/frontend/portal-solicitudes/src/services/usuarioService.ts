import { aQueryString, get, patch, post } from '../lib/apiClient';
import type { PaginaResultado, RolUsuario, UsuarioResumen } from '../types';

export interface FiltrosUsuarios {
  rol?: RolUsuario;
  soloActivos?: boolean;
  pagina?: number;
  tamanoPagina?: number;
}

/** Con `rol` definido, el backend ignora la paginacion y devuelve la lista completa (ver ObtenerUsuariosPaginadoQuery). */
export async function getUsuarios(filtros: FiltrosUsuarios = {}): Promise<PaginaResultado<UsuarioResumen>> {
  return get<PaginaResultado<UsuarioResumen>>(`/usuarios${aQueryString({ ...filtros })}`);
}

/** Fuente del selector de responsable al asignar una solicitud. */
export async function getAnalistas(): Promise<UsuarioResumen[]> {
  const pagina = await getUsuarios({ rol: 'Analista', soloActivos: true });
  return pagina.elementos;
}

export interface CrearUsuarioInput {
  nombre: string;
  email: string;
  password: string;
  rol: RolUsuario;
}

export async function crearUsuario(datos: CrearUsuarioInput): Promise<UsuarioResumen> {
  return post<UsuarioResumen>('/usuarios', datos);
}

export interface ActualizarUsuarioInput {
  nombre?: string;
  rol?: RolUsuario;
  activo?: boolean;
}

export async function actualizarUsuario(id: number, datos: ActualizarUsuarioInput): Promise<UsuarioResumen> {
  return patch<UsuarioResumen>(`/usuarios/${id}`, datos);
}
