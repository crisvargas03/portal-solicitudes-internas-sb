import { useEffect } from 'react';
import { obtenerUsuarioActual } from '../services/authService';
import { useAuthStore } from '../store/authStore';

/**
 * Al arrancar la app, si hay un token persistido se revalida contra GET /api/auth/me antes
 * de dejar decidir a RutaProtegida — evita que un token vencido muestre la app un instante
 * antes de expulsar al usuario. Si la llamada falla con 401, el interceptor de apiClient ya
 * se encarga de limpiar la sesión y redirigir; aquí solo se refresca el usuario si sigue vigente.
 */
export function useRevalidarSesion() {
  const token = useAuthStore((state) => state.token);
  const actualizarUsuario = useAuthStore((state) => state.actualizarUsuario);
  const setValidandoSesion = useAuthStore((state) => state.setValidandoSesion);

  useEffect(() => {
    if (!token) {
      setValidandoSesion(false);
      return;
    }

    let cancelado = false;

    obtenerUsuarioActual()
      .then((usuario) => {
        if (!cancelado) {
          actualizarUsuario(usuario);
        }
      })
      .catch(() => {
        // El interceptor 401 ya limpia la sesión y redirige; nada más que hacer aquí.
      })
      .finally(() => {
        if (!cancelado) {
          setValidandoSesion(false);
        }
      });

    return () => {
      cancelado = true;
    };
    // Solo debe correr una vez al montar la app con el token que había en ese momento.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
}
