import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  actualizarSolicitudCompleta,
  cambiarAsignacion,
  cambiarEstado,
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
  CambiarEstadoInput,
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

/** Transiciones válidas para esta solicitud y el rol de quien pregunta (ver GET .../transiciones). */
export function useTransiciones(id: number) {
  return useQuery({
    queryKey: ['solicitudes', id, 'transiciones'],
    queryFn: () => getTransiciones(id),
    enabled: Number.isFinite(id),
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

/** PATCH /api/solicitudes/{id}/estado: transiciones validadas en el servidor contra TransicionPermitida. */
export function useCambiarEstado() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: CambiarEstadoInput }) => cambiarEstado(id, datos),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['solicitudes', id] });
      queryClient.invalidateQueries({ queryKey: ['solicitudes', id, 'transiciones'] });
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

/** Comentario en el detalle de una solicitud, público o interno según el rol (ver ADR-0023). */
export function useCrearComentario() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, texto, esInterno }: { id: number; texto: string; esInterno: boolean }) =>
      crearComentario(id, texto, esInterno),
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
