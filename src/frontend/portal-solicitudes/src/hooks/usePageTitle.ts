import { matchPath, useLocation } from 'react-router';

const PAGE_TITLES: { pattern: string; title: string }[] = [
  { pattern: '/dashboard', title: 'Dashboard' },
  { pattern: '/solicitudes/nueva', title: 'Nueva solicitud' },
  { pattern: '/solicitudes/:id/editar', title: 'Editar solicitud' },
  { pattern: '/solicitudes/:id', title: 'Detalle de solicitud' },
  { pattern: '/solicitudes', title: 'Solicitudes' },
];

export function usePageTitle(): string {
  const { pathname } = useLocation();
  const match = PAGE_TITLES.find((entry) => matchPath({ path: entry.pattern, end: true }, pathname));
  return match?.title ?? 'Portal de Solicitudes';
}
