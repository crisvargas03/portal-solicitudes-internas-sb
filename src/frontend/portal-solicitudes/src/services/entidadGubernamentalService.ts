import { get, patch, post } from '../lib/apiClient';
import type { EntidadGubernamentalAdmin } from '../types';

// Catalogo administrable (ver ADR-0020 y ADR-0034): a diferencia de Areas/Prioridades/
// TiposSolicitud no vive en la base de datos relacional sino en un archivo de texto, pero
// la Api expone la misma forma admin (`/todas` con inactivas incluidas).

export interface GuardarEntidadGubernamentalInput {
  nombre?: string;
  categoria?: string;
  poderDelEstado?: string;
  sector?: string;
  activo?: boolean;
}

export async function getEntidadesGubernamentalesTodas(): Promise<EntidadGubernamentalAdmin[]> {
  return get<EntidadGubernamentalAdmin[]>('/entidades-gubernamentales/todas');
}

export async function crearEntidadGubernamental(datos: {
  nombre: string;
  categoria: string;
  poderDelEstado: string;
  sector: string;
}): Promise<EntidadGubernamentalAdmin> {
  return post<EntidadGubernamentalAdmin>('/entidades-gubernamentales', datos);
}

export async function actualizarEntidadGubernamental(
  id: number,
  datos: GuardarEntidadGubernamentalInput,
): Promise<EntidadGubernamentalAdmin> {
  return patch<EntidadGubernamentalAdmin>(`/entidades-gubernamentales/${id}`, datos);
}
