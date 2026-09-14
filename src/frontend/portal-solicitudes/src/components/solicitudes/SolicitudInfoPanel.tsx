import type { ReactNode } from 'react';
import { useState } from 'react';
import { toast } from 'sonner';
import { PriorityBadge } from './PriorityBadge';
import { StatusBadge } from './StatusBadge';
import { VencimientoIndicator } from './VencimientoIndicator';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';
import { inputClasses } from '../ui/inputClasses';
import { Modal } from '../ui/Modal';
import { useAnalistas } from '../../hooks/queries/useUsuarios';
import { useCambiarAsignacion } from '../../hooks/queries/useSolicitudes';
import { useConfirm } from '../../hooks/useConfirm';
import { ErrorApi } from '../../lib/apiClient';
import type { SolicitudDetalle, UsuarioResumen } from '../../types';

interface SolicitudInfoPanelProps {
  solicitud: SolicitudDetalle;
  /** Solo Administrador puede reasignar desde aquí (ver ADR-0012); el autoclaim de Analista vive en la cola. */
  puedeReasignar: boolean;
}

/** Panel lateral con los datos clave de la solicitud, siempre visibles sin scroll (sticky en desktop). */
export function SolicitudInfoPanel({ solicitud, puedeReasignar }: SolicitudInfoPanelProps) {
  const [modalAsignarAbierto, setModalAsignarAbierto] = useState(false);

  return (
    <Card className="h-fit lg:sticky lg:top-6">
      <div className="divide-y divide-slate-200">
        <CampoLateral etiqueta="Estado">
          <StatusBadge estado={solicitud.estado} />
        </CampoLateral>
        <CampoLateral etiqueta="Prioridad">
          <PriorityBadge prioridad={solicitud.prioridad} />
        </CampoLateral>
        <CampoLateral etiqueta="Responsable">
          <div className="flex items-center justify-between gap-2">
            <span>{solicitud.asignado?.nombre ?? 'Sin asignar'}</span>
            {puedeReasignar && (
              <button
                type="button"
                onClick={() => setModalAsignarAbierto(true)}
                className="shrink-0 text-xs font-normal text-accent-orange hover:underline"
              >
                Reasignar
              </button>
            )}
          </div>
        </CampoLateral>
        <CampoLateral etiqueta="Solicitante">{solicitud.solicitante.nombre}</CampoLateral>
        <CampoLateral etiqueta="Área">{solicitud.area.nombre}</CampoLateral>
        <CampoLateral etiqueta="Tipo de solicitud">{solicitud.tipoSolicitud.nombre}</CampoLateral>
        <CampoLateral etiqueta="Vencimiento">
          <VencimientoIndicator fechaCompromiso={solicitud.fechaCompromiso} estaVencida={solicitud.estaVencida} />
        </CampoLateral>
        <CampoLateral etiqueta="Creada">{new Date(solicitud.fechaCreacion).toLocaleDateString()}</CampoLateral>
      </div>

      {modalAsignarAbierto && (
        <AsignarModal
          solicitudId={solicitud.id}
          codigoSolicitud={solicitud.codigo}
          asignadoActual={solicitud.asignado}
          onCerrar={() => setModalAsignarAbierto(false)}
        />
      )}
    </Card>
  );
}

/** Fila etiqueta/valor — una sola forma para todos los datos clave de la solicitud. */
function CampoLateral({ etiqueta, children }: { etiqueta: string; children: ReactNode }) {
  return (
    <div className="flex flex-col gap-1 py-3 text-sm font-medium text-navy first:pt-0 last:pb-0">
      <p className="text-xs font-normal text-slate-500">{etiqueta}</p>
      {children}
    </div>
  );
}

interface AsignarModalProps {
  solicitudId: number;
  codigoSolicitud: string;
  asignadoActual: UsuarioResumen | null;
  onCerrar: () => void;
}

function AsignarModal({ solicitudId, codigoSolicitud, asignadoActual, onCerrar }: AsignarModalProps) {
  const { data: analistas = [] } = useAnalistas();
  const cambiarAsignacion = useCambiarAsignacion();
  const confirmar = useConfirm();
  const [analistaSeleccionado, setAnalistaSeleccionado] = useState(asignadoActual ? String(asignadoActual.id) : '');
  const [error, setError] = useState<string | null>(null);

  async function onSubmit() {
    setError(null);
    const idSeleccionado = analistaSeleccionado ? Number(analistaSeleccionado) : null;
    const sinCambios = idSeleccionado === (asignadoActual?.id ?? null);
    const accion = () => cambiarAsignacion.mutateAsync({ id: solicitudId, usuarioAsignadoId: idSeleccionado });
    try {
      // Solo se confirma cuando la solicitud le quita el trabajo a alguien que ya la tenía
      // (ver ADR-0033): la primera asignación desde "Sin asignar" no es destructiva.
      if (sinCambios || !asignadoActual) {
        await accion();
      } else if (idSeleccionado === null) {
        const confirmado = await confirmar({
          titulo: 'Quitar responsable',
          mensaje: `¿Quitar a «${asignadoActual.nombre}» como responsable de ${codigoSolicitud}? La solicitud quedará sin responsable y volverá a la lista de disponibles.`,
          variante: 'peligro',
          textoConfirmar: 'Quitar responsable',
          accion,
        });
        if (!confirmado) return;
      } else {
        const nuevoNombre = analistas.find((analista) => analista.id === idSeleccionado)?.nombre ?? 'otro analista';
        const confirmado = await confirmar({
          titulo: 'Reasignar solicitud',
          mensaje: `¿Pasar ${codigoSolicitud} de «${asignadoActual.nombre}» a «${nuevoNombre}»? Dejará de aparecer en la cola de ${asignadoActual.nombre}.`,
          textoConfirmar: 'Reasignar',
          accion,
        });
        if (!confirmado) return;
      }
      toast.success('Responsable actualizado');
      onCerrar();
    } catch (err) {
      const mensaje = err instanceof ErrorApi ? err.message : 'No se pudo asignar la solicitud.';
      setError(mensaje);
      toast.error(mensaje);
    }
  }

  return (
    <Modal abierto titulo="Reasignar responsable" onCerrar={onCerrar}>
      <div className="flex flex-col gap-3">
        {error && <p className="rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{error}</p>}
        <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
          Analista
          <select
            className={inputClasses(false)}
            value={analistaSeleccionado}
            onChange={(event) => setAnalistaSeleccionado(event.target.value)}
          >
            <option value="">Sin asignar</option>
            {analistas.map((analista) => (
              <option key={analista.id} value={analista.id}>
                {analista.nombre}
              </option>
            ))}
          </select>
        </label>
        <div className="flex justify-end gap-2">
          <Button type="button" variante="secundario" tamano="sm" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button type="button" tamano="sm" cargando={cambiarAsignacion.isPending} onClick={onSubmit}>
            Guardar
          </Button>
        </div>
      </div>
    </Modal>
  );
}
