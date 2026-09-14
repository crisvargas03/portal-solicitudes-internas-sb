import type {
  AdjuntoDetalle,
  ComentarioDetalle,
  PaginaResultado,
  Solicitud,
  SolicitudDetalle,
  TransicionDisponible,
} from '../types';
import { aQueryString, get, patch, post, put } from '../lib/apiClient';

/**
 * Corte adicional por asignacion dentro del alcance ya recortado por rol (ADR-0026/0027):
 * nunca sustituye el alcance del servidor, solo lo estrecha. Valores en PascalCase para que
 * el binding de enum del backend los resuelva igual que RolUsuario (ver usuarioService.ts).
 */
export type FiltroAsignacion = 'Todas' | 'Asignadas' | 'Disponibles';

/** Whitelist de ordenamiento server-side (ver ADR-0026) — nunca una columna arbitraria. */
export type OrdenSolicitudes = 'FechaCreacion' | 'Codigo' | 'Titulo' | 'Prioridad' | 'Urgencia';

export type DireccionOrden = 'Asc' | 'Desc';

export interface FiltrosSolicitudes {
  estadoId?: number;
  prioridadId?: number;
  areaId?: number;
  tipoSolicitudId?: number;
  usuarioSolicitanteId?: number;
  usuarioAsignadoId?: number;
  fechaCreacionDesde?: string;
  fechaCreacionHasta?: string;
  textoBusqueda?: string;
  soloVencidas?: boolean;
  asignacion?: FiltroAsignacion;
  orden?: OrdenSolicitudes;
  direccion?: DireccionOrden;
  pagina?: number;
  tamanoPagina?: number;
}

/**
 * GET /api/solicitudes: el alcance por rol (ADR-0012) ya viene aplicado en el servidor antes
 * de estos filtros — nunca hace falta (ni sirve) mandar usuarioSolicitanteId/usuarioAsignadoId
 * para acotar a "lo mio", el servidor los ignora si intentan ampliar el alcance.
 */
export async function getSolicitudes(filtros: FiltrosSolicitudes = {}): Promise<PaginaResultado<Solicitud>> {
  return get<PaginaResultado<Solicitud>>(`/solicitudes${aQueryString({ ...filtros })}`);
}

/** GET /api/solicitudes/{id}: detalle real, con historial, comentarios y adjuntos ya filtrados por rol. */
export async function getSolicitudById(id: number): Promise<SolicitudDetalle> {
  return get<SolicitudDetalle>(`/solicitudes/${id}`);
}

/** Vista Solicitante (ADR-0012): el servidor ya acota a las propias, sin parametro adicional. */
export async function getMisSolicitudes(filtros: FiltrosSolicitudes = {}): Promise<PaginaResultado<Solicitud>> {
  return getSolicitudes(filtros);
}

/** Cola de Analista, grupo "Asignadas a mí" (ADR-0026). */
export async function getAsignadas(filtros: FiltrosSolicitudes = {}): Promise<PaginaResultado<Solicitud>> {
  return getSolicitudes({ ...filtros, asignacion: 'Asignadas' });
}

/** Cola de Analista, grupo "Disponibles para tomar" — sin responsable asignado (ADR-0026). */
export async function getDisponibles(filtros: FiltrosSolicitudes = {}): Promise<PaginaResultado<Solicitud>> {
  return getSolicitudes({ ...filtros, asignacion: 'Disponibles' });
}

/**
 * GET /api/solicitudes/{id}/transiciones: transiciones válidas para el estado actual de la
 * solicitud y el rol de quien pregunta — ya filtradas en el servidor (ver ADR-0005), nunca una
 * lista libre que el cliente deba recortar.
 */
export async function getTransiciones(id: number): Promise<TransicionDisponible[]> {
  return get<TransicionDisponible[]>(`/solicitudes/${id}/transiciones`);
}

export interface CambiarEstadoInput {
  estadoDestinoId: number;
  comentario?: string;
}

/** PATCH /api/solicitudes/{id}/estado: el comentario es obligatorio o no según la transición elegida. */
export async function cambiarEstado(id: number, datos: CambiarEstadoInput): Promise<Solicitud> {
  return patch<Solicitud>(`/solicitudes/${id}/estado`, datos);
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
 * POST /api/solicitudes/{id}/comentarios. `esInterno` lo decide quien llama (ver ADR-0023):
 * el servidor igual lo fuerza a `false` si el rol es Solicitante, así que el checkbox correspondiente
 * ni siquiera se renderiza para ese rol (ver SolicitudDetail.tsx) — esto es solo coherencia de cliente.
 */
export async function crearComentario(id: number, texto: string, esInterno: boolean): Promise<ComentarioDetalle> {
  return post<ComentarioDetalle>(`/solicitudes/${id}/comentarios`, { texto, esInterno });
}

export interface CrearAdjuntoInput {
  descripcion: string;
  url: string;
}

/** POST /api/solicitudes/{id}/adjuntos: referencia de texto/URL, sin archivos (ver ADR-0006). */
export async function crearAdjunto(id: number, datos: CrearAdjuntoInput): Promise<AdjuntoDetalle> {
  return post<AdjuntoDetalle>(`/solicitudes/${id}/adjuntos`, datos);
}
