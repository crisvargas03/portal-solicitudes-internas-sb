import type {
  AdjuntoDetalle,
  ComentarioDetalle,
  ResumenDashboard,
  RolUsuario,
  Solicitud,
  SolicitudDetalle,
  TransicionDisponible,
} from '../types';
import { construirResumenDashboard } from '../mocks/dashboard';
import { MOCK_SOLICITUDES } from '../mocks/solicitudes';
import { obtenerTransicionesDisponibles } from '../mocks/transiciones';
import { get, patch, post, put } from '../lib/apiClient';

// Todo lo de este archivo (salvo el detalle y las funciones al final) opera sobre
// MOCK_SOLICITUDES. Cuando se conecte la API real para lista/dashboard, solo este archivo
// cambia — el resto de la app consume los hooks de useSolicitudes.ts.
// getSolicitudById, crearSolicitud, crearComentario, crearAdjunto, actualizarSolicitudCompleta
// y cambiarAsignacion ya llaman a la API real: el detalle y sus mutaciones no tienen
// equivalente razonable en el mock (comentarios/adjuntos con autor y fecha reales).

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

/** GET /api/solicitudes/{id}: detalle real, con historial, comentarios y adjuntos ya filtrados por rol. */
export async function getSolicitudById(id: number): Promise<SolicitudDetalle> {
  return get<SolicitudDetalle>(`/solicitudes/${id}`);
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

/**
 * Transiciones válidas para el estado actual y el rol dado — ya filtradas, nunca una lista libre.
 * Sigue operando sobre MOCK_SOLICITUDES (no sobre getSolicitudById, que ya es la API real):
 * conectar el endpoint real GET /api/solicitudes/{id}/transiciones queda fuera de este cambio.
 */
export async function getTransiciones(id: number, rol: RolUsuario): Promise<TransicionDisponible[]> {
  const solicitud = MOCK_SOLICITUDES.find((solicitud) => solicitud.id === id);
  if (!solicitud?.estado) return [];
  return obtenerTransicionesDisponibles(solicitud.estado.codigo, rol);
}

export async function getResumenDashboard(filtros: FiltrosSolicitudes = {}): Promise<ResumenDashboard> {
  return construirResumenDashboard(aplicarFiltros(MOCK_SOLICITUDES, filtros));
}

export interface ActualizarSolicitudCompletaInput {
  titulo: string;
  descripcion: string;
  tipoSolicitudId: number;
  prioridadId: number;
  areaId: number;
}

/**
 * PUT /api/solicitudes/{id}: edicion completa exclusiva de Administrador (ver ADR-0021).
 * Tipa el retorno como `Solicitud` de forma aproximada — el DTO real (SolicitudResumenDto)
 * no trae los *Id planos, solo los objetos anidados; no hay consumidor hoy que dependa de eso.
 */
export async function actualizarSolicitudCompleta(id: number, datos: ActualizarSolicitudCompletaInput): Promise<Solicitud> {
  return put<Solicitud>(`/solicitudes/${id}`, datos);
}

/** PATCH /api/solicitudes/{id}/asignacion: usuarioAsignadoId nulo desasigna (ver ADR-0012). */
export async function cambiarAsignacion(id: number, usuarioAsignadoId: number | null): Promise<Solicitud> {
  return patch<Solicitud>(`/solicitudes/${id}/asignacion`, { usuarioAsignadoId });
}

export interface CrearSolicitudInput {
  titulo: string;
  descripcion: string;
  tipoSolicitudId: number;
  prioridadId: number;
  areaId: number;
}

/** POST /api/solicitudes: el solicitante sale del token, no se envía en el cuerpo. */
export async function crearSolicitud(datos: CrearSolicitudInput): Promise<Solicitud> {
  return post<Solicitud>('/solicitudes', datos);
}

/**
 * POST /api/solicitudes/{id}/comentarios. `esInterno` siempre viaja en `false`: el formulario
 * de comentarios se comparte entre roles (ver SolicitudDetail.tsx) y ninguno de ellos tiene hoy
 * un control para marcar un comentario como interno — el servidor lo forzaría de todas formas
 * si el rol es Solicitante (ver ADR-0023), pero para Admin/Analista esto significa que sus
 * comentarios desde esta pantalla también son siempre públicos hasta que se agregue ese control.
 */
export async function crearComentario(id: number, texto: string): Promise<ComentarioDetalle> {
  return post<ComentarioDetalle>(`/solicitudes/${id}/comentarios`, { texto, esInterno: false });
}

export interface CrearAdjuntoInput {
  descripcion: string;
  url: string;
}

/** POST /api/solicitudes/{id}/adjuntos: referencia de texto/URL, sin archivos (ver ADR-0006). */
export async function crearAdjunto(id: number, datos: CrearAdjuntoInput): Promise<AdjuntoDetalle> {
  return post<AdjuntoDetalle>(`/solicitudes/${id}/adjuntos`, datos);
}
