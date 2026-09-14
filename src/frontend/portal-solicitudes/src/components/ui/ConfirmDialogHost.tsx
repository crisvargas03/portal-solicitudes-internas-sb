import { useConfirmStore } from '../../store/confirmStore';
import { ConfirmDialog } from './ConfirmDialog';

/** Punto único de montaje del ConfirmDialog, leído desde confirmStore (ver ADR-0033). Se monta
 * una vez en App.tsx, junto al <Toaster />, para que useConfirm() no necesite prop-drilling. */
export function ConfirmDialogHost() {
  const solicitud = useConfirmStore((state) => state.solicitud);
  const cargando = useConfirmStore((state) => state.cargando);
  const setCargando = useConfirmStore((state) => state.setCargando);
  const cerrar = useConfirmStore((state) => state.cerrar);

  async function manejarConfirmar() {
    if (!solicitud) return;
    if (!solicitud.accion) {
      solicitud.resolver(true);
      cerrar();
      return;
    }
    setCargando(true);
    try {
      await solicitud.accion();
      solicitud.resolver(true);
      cerrar();
    } catch (error) {
      // El diálogo se cierra igual; el error se propaga al try/catch del llamador de confirm()
      // (ver ADR-0033) en vez de manejarse aquí.
      cerrar();
      solicitud.rechazar(error);
    }
  }

  function manejarCancelar() {
    if (!solicitud) return;
    solicitud.resolver(false);
    cerrar();
  }

  return (
    <ConfirmDialog
      abierto={solicitud !== null}
      titulo={solicitud?.titulo ?? ''}
      mensaje={solicitud?.mensaje}
      textoConfirmar={solicitud?.textoConfirmar}
      textoCancelar={solicitud?.textoCancelar}
      variante={solicitud?.variante}
      cargando={cargando}
      onConfirmar={() => void manejarConfirmar()}
      onCancelar={manejarCancelar}
    />
  );
}
