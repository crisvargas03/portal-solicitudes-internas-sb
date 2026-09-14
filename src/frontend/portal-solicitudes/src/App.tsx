import { Toaster } from 'sonner';
import { RouterProvider } from 'react-router';
import { ConfirmDialogHost } from './components/ui/ConfirmDialogHost';
import { useRenovacionSesion } from './hooks/useRenovacionSesion';
import { useRevalidarSesion } from './hooks/useRevalidarSesion';
import { router } from './router';

/** Punto único donde se revalida la sesión persistida antes de que el router decida rutas. */
export function App() {
  useRevalidarSesion();
  useRenovacionSesion();

  return (
    <>
      <Toaster richColors position="top-right" />
      <ConfirmDialogHost />
      <RouterProvider router={router} />
    </>
  );
}
