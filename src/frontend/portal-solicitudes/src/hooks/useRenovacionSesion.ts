import { useEffect } from 'react';
import { renovarSesion } from '../services/authService';
import { useAuthStore } from '../store/authStore';

// Renovar unos minutos antes de que venza, no justo al vencer — deja margen para la
// llamada en vuelo y para relojes ligeramente desincronizados entre cliente y servidor.
const MS_UMBRAL_RENOVACION = 10 * 60 * 1000; // 10 minutos
const MS_REINTENTO_MINIMO = 5_000;

/**
 * Renovación proactiva por re-emisión deslizante (ver ADR-0019): mientras haya sesión,
 * programa un POST /api/auth/refresh para poco antes de que el token venza, y vuelve a
 * programar el siguiente tras cada renovación exitosa. Si la renovación falla (el token
 * ya venció o el usuario fue desactivado), cierra la sesión una sola vez — /auth/refresh
 * está exento del logout automático del interceptor para evitar una cascada.
 */
export function useRenovacionSesion() {
  const token = useAuthStore((state) => state.token);
  const expiraEn = useAuthStore((state) => state.expiraEn);
  const actualizarToken = useAuthStore((state) => state.actualizarToken);
  const logout = useAuthStore((state) => state.logout);

  useEffect(() => {
    if (!token || !expiraEn) {
      return;
    }

    const msHastaRenovar = Math.max(
      new Date(expiraEn).getTime() - Date.now() - MS_UMBRAL_RENOVACION,
      MS_REINTENTO_MINIMO,
    );

    const idTimeout = window.setTimeout(async () => {
      try {
        const sesion = await renovarSesion();
        actualizarToken(sesion.token, sesion.expiraEn);
      } catch {
        logout();
        window.location.assign('/login?sesionExpirada=1');
      }
    }, msHastaRenovar);

    return () => window.clearTimeout(idTimeout);
  }, [token, expiraEn, actualizarToken, logout]);
}
