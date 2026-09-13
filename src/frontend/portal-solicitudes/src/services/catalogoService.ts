import { get } from '../lib/apiClient';
import type { Area, EstadoSolicitud, Prioridad, TipoSolicitud } from '../types';

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
