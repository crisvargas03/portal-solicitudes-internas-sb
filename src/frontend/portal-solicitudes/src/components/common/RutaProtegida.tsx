import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router';
import { useAuthStore } from '../../store/authStore';

interface RutaProtegidaProps {
  children: ReactNode;
}

/**
 * Envuelve la rama de rutas que exige sesión. Sin usuario, redirige a /login guardando la
 * ruta que se intentaba abrir (`volverA`) para volver ahí después de iniciar sesión.
 *
 * Mientras se revalida un token persistido contra GET /api/auth/me al arrancar
 * (`validandoSesion`), no decide nada todavía — evita el parpadeo a /login en cada refresh.
 */
export function RutaProtegida({ children }: RutaProtegidaProps) {
  const user = useAuthStore((state) => state.user);
  const validandoSesion = useAuthStore((state) => state.validandoSesion);
  const location = useLocation();

  if (validandoSesion) {
    return null;
  }

  if (!user) {
    const volverA = encodeURIComponent(location.pathname + location.search);
    return <Navigate to={`/login?volverA=${volverA}`} replace />;
  }

  return <>{children}</>;
}
