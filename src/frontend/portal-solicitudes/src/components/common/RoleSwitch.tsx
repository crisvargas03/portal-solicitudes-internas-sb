import type { ReactNode } from 'react';
import { useRolActual } from '../../hooks/useRolActual';

interface RoleSwitchProps {
  administrador: ReactNode;
  analista: ReactNode;
  solicitante: ReactNode;
}

/** Mismas rutas para todos los roles; esto decide qué composición se renderiza en cada una. */
export function RoleSwitch({ administrador, analista, solicitante }: RoleSwitchProps) {
  const { esAdministrador, esAnalista } = useRolActual();
  if (esAdministrador) return <>{administrador}</>;
  if (esAnalista) return <>{analista}</>;
  return <>{solicitante}</>;
}
