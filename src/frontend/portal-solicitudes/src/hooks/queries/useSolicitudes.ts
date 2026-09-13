import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { RolUsuario } from '../../types';
import {
  actualizarSolicitudCompleta,
  cambiarAsignacion,
  crearAdjunto,
  crearComentario,
  crearSolicitud,
  getAsignadas,
  getDisponibles,
  getMisSolicitudes,
  getSolicitudById,
  getSolicitudes,
  getTransiciones,
} from '../../services/solicitudService';
import type {
  ActualizarSolicitudCompletaInput,
  CrearAdjuntoInput,
  CrearSolicitudInput,
  FiltrosSolicitudes,
} from '../../services/solicitudService';

export function useSolicitudes(filtros: FiltrosSolicitudes = {}) {
  return useQuery({ queryKey: ['solicitudes', filtros], queryFn: () => getSolicitudes(filtros) });
}

export function useSolicitud(id: number) {
  return useQuery({
    queryKey: ['solicitudes', id],
    queryFn: () => getSolicitudById(id),
    enabled: Number.isFinite(id),
  });
}

/** Vista Solicitante (ADR-0012): el servidor deriva "las mias" del token, sin usuarioId explicito. */
export function useMisSolicitudes(filtros: FiltrosSolicitudes = {}) {
  return useQuery({ queryKey: ['solicitudes', 'mias', filtros], queryFn: () => getMisSolicitudes(filtros) });
}

/** Cola de Analista, grupo "Asignadas a mí". */
export function useSolicitudesAsignadas(filtros: FiltrosSolicitudes = {}) {
  return useQuery({ queryKey: ['solicitudes', 'asignadas', filtros], queryFn: () => getAsignadas(filtros) });
}

/** Cola de Analista, grupo "Disponibles para tomar". */
export function useSolicitudesDisponibles(filtros: FiltrosSolicitudes = {}) {
  return useQuery({ queryKey: ['solicitudes', 'disponibles', filtros], queryFn: () => getDisponibles(filtros) });
}

export function useTransiciones(estadoActualCodigo: string | undefined, rol: RolUsuario | undefined) {
  return useQuery({
    queryKey: ['solicitudes', 'transiciones', estadoActualCodigo, rol],
    queryFn: () => getTransiciones(estadoActualCodigo as string, rol as RolUsuario),
    enabled: Boolean(estadoActualCodigo) && Boolean(rol),
  });
}

/** Edicion completa de Administrador (ver ADR-0021). */
export function useActualizarSolicitudCompleta() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: ActualizarSolicitudCompletaInput }) =>
      actualizarSolicitudCompleta(id, datos),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['solicitudes', id] });
      queryClient.invalidateQueries({ queryKey: ['solicitudes'] });
    },
  });
}

export function useCambiarAsignacion() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, usuarioAsignadoId }: { id: number; usuarioAsignadoId: number | null }) =>
      cambiarAsignacion(id, usuarioAsignadoId),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['solicitudes', id] });
      queryClient.invalidateQueries({ queryKey: ['solicitudes'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

/** Alta de Solicitante/Analista (ver ADR-0025 para la evidencia opcional encadenada). */
export function useCrearSolicitud() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (datos: CrearSolicitudInput) => crearSolicitud(datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['solicitudes'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

/** Comentario público en el detalle de una solicitud (ver ADR-0023). */
export function useCrearComentario() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, texto }: { id: number; texto: string }) => crearComentario(id, texto),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['solicitudes', id] });
    },
  });
}

/** Referencia de evidencia (texto/URL) agregada desde el detalle de una solicitud (ver ADR-0006). */
export function useCrearAdjunto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: CrearAdjuntoInput }) => crearAdjunto(id, datos),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['solicitudes', id] });
    },
  });
}
