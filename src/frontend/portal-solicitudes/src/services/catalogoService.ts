import { get, patch, post } from '../lib/apiClient';
import type { Area, AreaAdmin, EstadoSolicitud, Prioridad, PrioridadAdmin, TipoSolicitud, TipoSolicitudAdmin } from '../types';

// Los cuatro catalogos son [Authorize] en el backend: solo cargan con sesion activa.
// Cambian rarisimo (ver hooks/queries/useCatalogos.ts), asi que este archivo es la unica
// pieza que cambiaria si algun dia se agrega administracion de catalogos.

export async function getAreas(): Promise<Area[]> {
  return get<Area[]>('/areas');
}

export async function getTiposSolicitud(): Promise<TipoSolicitud[]> {
  return get<TipoSolicitud[]>('/tipos-solicitud');
}

export async function getPrioridades(): Promise<Prioridad[]> {
  return get<Prioridad[]>('/prioridades');
}

export async function getEstadosSolicitud(): Promise<EstadoSolicitud[]> {
  return get<EstadoSolicitud[]>('/estados-solicitud');
}

// A partir de aqui: administracion de catalogos (Administrador), agregada junto al resto
// del CRUD de Admin (ver ADR-0020). Áreas, Prioridades y TipoSolicitud son administrables;
// Estados sigue siendo un flujo fijo sin pantalla de administracion.

export interface GuardarAreaInput {
  nombre?: string;
  activo?: boolean;
}

export async function getAreasTodas(): Promise<AreaAdmin[]> {
  return get<AreaAdmin[]>('/areas/todas');
}

export async function crearArea(datos: { nombre: string }): Promise<AreaAdmin> {
  return post<AreaAdmin>('/areas', datos);
}

export async function actualizarArea(id: number, datos: GuardarAreaInput): Promise<AreaAdmin> {
  return patch<AreaAdmin>(`/areas/${id}`, datos);
}

export interface GuardarPrioridadInput {
  nombre?: string;
  nivel?: number;
  activo?: boolean;
}

export async function getPrioridadesTodas(): Promise<PrioridadAdmin[]> {
  return get<PrioridadAdmin[]>('/prioridades/todas');
}

export async function crearPrioridad(datos: { nombre: string; nivel: number }): Promise<PrioridadAdmin> {
  return post<PrioridadAdmin>('/prioridades', datos);
}

export async function actualizarPrioridad(id: number, datos: GuardarPrioridadInput): Promise<PrioridadAdmin> {
  return patch<PrioridadAdmin>(`/prioridades/${id}`, datos);
}

export interface GuardarTipoSolicitudInput {
  nombre?: string;
  descripcion?: string | null;
  activo?: boolean;
}

export async function getTiposSolicitudTodos(): Promise<TipoSolicitudAdmin[]> {
  return get<TipoSolicitudAdmin[]>('/tipos-solicitud/todos');
}

export async function crearTipoSolicitud(datos: { nombre: string; descripcion?: string | null }): Promise<TipoSolicitudAdmin> {
  return post<TipoSolicitudAdmin>('/tipos-solicitud', datos);
}

export async function actualizarTipoSolicitud(id: number, datos: GuardarTipoSolicitudInput): Promise<TipoSolicitudAdmin> {
  return patch<TipoSolicitudAdmin>(`/tipos-solicitud/${id}`, datos);
}
