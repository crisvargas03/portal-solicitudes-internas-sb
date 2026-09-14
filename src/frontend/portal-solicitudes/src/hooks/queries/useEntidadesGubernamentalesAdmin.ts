import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  actualizarEntidadGubernamental,
  crearEntidadGubernamental,
  getEntidadesGubernamentalesTodas,
} from '../../services/entidadGubernamentalService';
import type { GuardarEntidadGubernamentalInput } from '../../services/entidadGubernamentalService';

// Lista de administracion (incluye inactivas): se refresca tras cada mutacion, igual que el
// resto de catalogos administrables (ver useCatalogosAdmin.ts).

export function useEntidadesGubernamentalesTodas() {
  return useQuery({ queryKey: ['entidadesGubernamentales', 'todas'], queryFn: getEntidadesGubernamentalesTodas });
}

export function useCrearEntidadGubernamental() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (datos: { nombre: string; categoria: string; poderDelEstado: string; sector: string }) =>
      crearEntidadGubernamental(datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['entidadesGubernamentales'] });
    },
  });
}

export function useActualizarEntidadGubernamental() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: GuardarEntidadGubernamentalInput }) =>
      actualizarEntidadGubernamental(id, datos),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['entidadesGubernamentales'] });
    },
  });
}
