import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { RolUsuario } from '../../types';
import {
  actualizarSolicitudCompleta,
  cambiarAsignacion,
  getAsignadas,
  getDisponibles,
  getMisSolicitudes,
  getResumenDashboard,
  getSolicitudById,
  getSolicitudes,
  getTransiciones,
} from '../../services/solicitudService';
import type { ActualizarSolicitudCompletaInput, FiltrosSolicitudes } from '../../services/solicitudService';

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

/** Vista Solicitante. */
export function useMisSolicitudes(usuarioSolicitanteId: number, filtros: FiltrosSolicitudes = {}) {
  return useQuery({
    queryKey: ['solicitudes', 'mias', usuarioSolicitanteId, filtros],
    queryFn: () => getMisSolicitudes(usuarioSolicitanteId, filtros),
    enabled: Number.isFinite(usuarioSolicitanteId) && usuarioSolicitanteId > 0,
  });
}

/** Cola de Analista, grupo "Asignadas a mí". */
export function useSolicitudesAsignadas(usuarioAsignadoId: number, filtros: FiltrosSolicitudes = {}) {
  return useQuery({
    queryKey: ['solicitudes', 'asignadas', usuarioAsignadoId, filtros],
    queryFn: () => getAsignadas(usuarioAsignadoId, filtros),
    enabled: Number.isFinite(usuarioAsignadoId) && usuarioAsignadoId > 0,
  });
}

/** Cola de Analista, grupo "Disponibles para tomar". */
export function useSolicitudesDisponibles(filtros: FiltrosSolicitudes = {}) {
  return useQuery({ queryKey: ['solicitudes', 'disponibles', filtros], queryFn: () => getDisponibles(filtros) });
}

export function useTransiciones(id: number, rol: RolUsuario | undefined) {
  return useQuery({
    queryKey: ['solicitudes', id, 'transiciones', rol],
    queryFn: () => getTransiciones(id, rol as RolUsuario),
    enabled: Number.isFinite(id) && Boolean(rol),
  });
}

export function useResumenDashboard(filtros: FiltrosSolicitudes = {}) {
  return useQuery({ queryKey: ['dashboard', 'resumen', filtros], queryFn: () => getResumenDashboard(filtros) });
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
    },
  });
}
