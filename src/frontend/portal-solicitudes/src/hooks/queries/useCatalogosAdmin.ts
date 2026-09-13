import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  actualizarArea,
  actualizarPrioridad,
  actualizarTipoSolicitud,
  crearArea,
  crearPrioridad,
  crearTipoSolicitud,
  getAreasTodas,
  getPrioridadesTodas,
  getTiposSolicitudTodos,
} from '../../services/catalogoService';
import type { GuardarAreaInput, GuardarPrioridadInput, GuardarTipoSolicitudInput } from '../../services/catalogoService';

// Listas de administracion (incluyen inactivos): a diferencia de useAreas/usePrioridades/
// useTiposSolicitud, aqui si conviene refrescar tras cada mutacion en vez de staleTime: Infinity.

export function useAreasTodas() {
  return useQuery({ queryKey: ['catalogos', 'areas', 'todas'], queryFn: getAreasTodas });
}

export function useCrearArea() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (datos: { nombre: string }) => crearArea(datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['catalogos', 'areas'] });
    },
  });
}

export function useActualizarArea() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: GuardarAreaInput }) => actualizarArea(id, datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['catalogos', 'areas'] });
    },
  });
}

export function usePrioridadesTodas() {
  return useQuery({ queryKey: ['catalogos', 'prioridades', 'todas'], queryFn: getPrioridadesTodas });
}

export function useCrearPrioridad() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (datos: { nombre: string; nivel: number }) => crearPrioridad(datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['catalogos', 'prioridades'] });
    },
  });
}

export function useActualizarPrioridad() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: GuardarPrioridadInput }) => actualizarPrioridad(id, datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['catalogos', 'prioridades'] });
    },
  });
}

export function useTiposSolicitudTodos() {
  return useQuery({ queryKey: ['catalogos', 'tiposSolicitud', 'todos'], queryFn: getTiposSolicitudTodos });
}

export function useCrearTipoSolicitud() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (datos: { nombre: string; descripcion?: string | null }) => crearTipoSolicitud(datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['catalogos', 'tiposSolicitud'] });
    },
  });
}

export function useActualizarTipoSolicitud() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: GuardarTipoSolicitudInput }) => actualizarTipoSolicitud(id, datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['catalogos', 'tiposSolicitud'] });
    },
  });
}
