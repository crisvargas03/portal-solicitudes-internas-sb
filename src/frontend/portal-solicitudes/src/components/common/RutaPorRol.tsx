import type { ReactNode } from 'react';
import { Navigate } from 'react-router';
import { useRolActual } from '../../hooks/useRolActual';
import type { RolUsuario } from '../../types';

interface RutaPorRolProps {
  rolesPermitidos: RolUsuario[];
  children: ReactNode;
}

/**
 * Restringe una ruta ya autenticada (usar dentro de RutaProtegida) a los roles indicados.
 * Un rol no permitido vuelve a /dashboard. Es el mecanismo reservado para /usuarios y
 * /catalogos el día que existan como pantallas de administración (ver ADR-0016).
 */
export function RutaPorRol({ rolesPermitidos, children }: RutaPorRolProps) {
  const { rol } = useRolActual();

  if (!rol || !rolesPermitidos.includes(rol)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}
