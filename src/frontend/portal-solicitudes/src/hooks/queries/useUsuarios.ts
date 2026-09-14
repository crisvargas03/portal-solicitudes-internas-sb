import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { actualizarUsuario, crearUsuario, getAnalistas, getUsuarios } from '../../services/usuarioService';
import type { ActualizarUsuarioInput, CrearUsuarioInput, FiltrosUsuarios } from '../../services/usuarioService';

export function useUsuarios(filtros: FiltrosUsuarios = {}) {
  return useQuery({ queryKey: ['usuarios', filtros], queryFn: () => getUsuarios(filtros) });
}

/** Fuente del selector de responsable al asignar una solicitud (ver PATCH .../asignacion). */
export function useAnalistas() {
  return useQuery({ queryKey: ['usuarios', 'analistas'], queryFn: getAnalistas });
}

export function useCrearUsuario() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (datos: CrearUsuarioInput) => crearUsuario(datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] });
    },
  });
}

export function useActualizarUsuario() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: ActualizarUsuarioInput }) => actualizarUsuario(id, datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] });
    },
  });
}
