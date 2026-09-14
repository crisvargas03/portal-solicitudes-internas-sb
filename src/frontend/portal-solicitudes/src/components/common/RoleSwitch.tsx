import type { ReactNode } from 'react';
import { Navigate } from 'react-router';
import { useRolActual } from '../../hooks/useRolActual';

interface RoleSwitchProps {
  administrador: ReactNode;
  analista: ReactNode;
  solicitante: ReactNode;
}

/**
 * Mismas rutas para todos los roles; esto decide qué composición se renderiza en cada una.
 * Sin sesión no hay rama por defecto: `RutaProtegida` ya debería impedir llegar aquí sin
 * usuario, pero este `Navigate` es la defensa por si algún día una ruta olvida el guard —
 * antes caía silenciosamente en la vista de Solicitante.
 */
export function RoleSwitch({ administrador, analista, solicitante }: RoleSwitchProps) {
  const { esAdministrador, esAnalista, esSolicitante } = useRolActual();
  if (esAdministrador) return <>{administrador}</>;
  if (esAnalista) return <>{analista}</>;
  if (esSolicitante) return <>{solicitante}</>;
  return <Navigate to="/login" replace />;
}
