import type { ResumenDashboard, RolUsuario, Solicitud, TransicionDisponible } from '../types';
import { construirResumenDashboard } from '../mocks/dashboard';
import { MOCK_SOLICITUDES } from '../mocks/solicitudes';
import { obtenerTransicionesDisponibles } from '../mocks/transiciones';

// Todo lo de este archivo opera sobre MOCK_SOLICITUDES. Cuando se conecte la API real,
// solo este archivo cambia — el resto de la app consume los hooks de useSolicitudes.ts.

export interface FiltrosSolicitudes {
  estadoCodigo?: string;
  prioridadId?: number;
  areaId?: number;
  tipoSolicitudId?: number;
  usuarioSolicitanteId?: number;
  usuarioAsignadoId?: number;
  fechaDesde?: string;
  fechaHasta?: string;
  soloVencidas?: boolean;
}

function aplicarFiltros(solicitudes: Solicitud[], filtros: FiltrosSolicitudes = {}): Solicitud[] {
  return solicitudes.filter((solicitud) => {
    if (filtros.estadoCodigo && solicitud.estado?.codigo !== filtros.estadoCodigo) return false;
    if (filtros.prioridadId && solicitud.prioridadId !== filtros.prioridadId) return false;
    if (filtros.areaId && solicitud.areaId !== filtros.areaId) return false;
    if (filtros.tipoSolicitudId && solicitud.tipoSolicitudId !== filtros.tipoSolicitudId) return false;
    if (filtros.usuarioSolicitanteId && solicitud.usuarioSolicitanteId !== filtros.usuarioSolicitanteId) return false;
    if (filtros.usuarioAsignadoId && solicitud.usuarioAsignadoId !== filtros.usuarioAsignadoId) return false;
    if (filtros.fechaDesde && solicitud.fechaCreacion < filtros.fechaDesde) return false;
    if (filtros.fechaHasta && solicitud.fechaCreacion > filtros.fechaHasta) return false;
    if (filtros.soloVencidas && !solicitud.estaVencida) return false;
    return true;
  });
}

export async function getSolicitudes(filtros: FiltrosSolicitudes = {}): Promise<Solicitud[]> {
  return aplicarFiltros(MOCK_SOLICITUDES, filtros);
}

export async function getSolicitudById(id: number): Promise<Solicitud | undefined> {
  return MOCK_SOLICITUDES.find((solicitud) => solicitud.id === id);
}

/** Vista Solicitante: ADR-0012 — solo sus propias solicitudes. */
export async function getMisSolicitudes(usuarioSolicitanteId: number, filtros: FiltrosSolicitudes = {}): Promise<Solicitud[]> {
  return aplicarFiltros(MOCK_SOLICITUDES, { ...filtros, usuarioSolicitanteId });
}

/** Vista Analista, grupo "Asignadas a mí". */
export async function getAsignadas(usuarioAsignadoId: number, filtros: FiltrosSolicitudes = {}): Promise<Solicitud[]> {
  return aplicarFiltros(MOCK_SOLICITUDES, { ...filtros, usuarioAsignadoId });
}

/** Vista Analista, grupo "Disponibles para tomar" — sin responsable asignado. */
export async function getDisponibles(filtros: FiltrosSolicitudes = {}): Promise<Solicitud[]> {
  return aplicarFiltros(
    MOCK_SOLICITUDES.filter((solicitud) => !solicitud.usuarioAsignadoId),
    filtros,
  );
}

/** Transiciones válidas para el estado actual y el rol dado — ya filtradas, nunca una lista libre. */
export async function getTransiciones(id: number, rol: RolUsuario): Promise<TransicionDisponible[]> {
  const solicitud = await getSolicitudById(id);
  if (!solicitud?.estado) return [];
  return obtenerTransicionesDisponibles(solicitud.estado.codigo, rol);
}

export async function getResumenDashboard(filtros: FiltrosSolicitudes = {}): Promise<ResumenDashboard> {
  return construirResumenDashboard(aplicarFiltros(MOCK_SOLICITUDES, filtros));
}
