import { useAuthStore } from '../store/authStore';

/**
 * Único punto de lectura de user.rol fuera de authStore — todo componente que
 * necesite ramificar por rol debe pasar por aquí en lugar de leer el store directo.
 */
export function useRolActual() {
  const rol = useAuthStore((state) => state.user?.rol);

  return {
    rol,
    esAdministrador: rol === 'Administrador',
    esAnalista: rol === 'Analista',
    esSolicitante: rol === 'Solicitante',
  };
}
