import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { Button } from '../ui/Button';
import { FormField } from '../ui/FormField';
import { Modal } from '../ui/Modal';
import { inputClasses } from '../ui/inputClasses';
import { useCambiarEstado, useTransiciones } from '../../hooks/queries/useSolicitudes';
import { ErrorApi } from '../../lib/apiClient';
import { cambiarEstadoSchema, type CambiarEstadoFormValues } from '../../schemas/cambiarEstadoSchema';
import { CodigosEstadoSolicitud } from '../../types';
import type { TransicionDisponible } from '../../types';

interface CambiarEstadoModalProps {
  solicitudId: number;
  onCerrar: () => void;
}

/**
 * Formulario compartido por la cola de Analista y el detalle de solicitud. Quien lo usa debe
 * montarlo condicionalmente (solo mientras la acción está abierta, ver AnalistaQueueView) para
 * que cada apertura arranque con estado fresco — este componente no se reinicia a sí mismo.
 *
 * El `<select>` solo ofrece los destinos que GET .../transiciones ya filtró por estado actual y
 * rol (ver ADR-0005) — nunca una lista libre de estados. El comentario se vuelve obligatorio
 * según `transicion.requiereComentario`, nunca comparando el destino contra CERRADA a mano: esa
 * restricción vive en la transicion EN_PROGRESO → RESUELTA (ver ADR-0001/0002).
 */
export function CambiarEstadoModal({ solicitudId, onCerrar }: CambiarEstadoModalProps) {
  const { data: transiciones = [], isLoading } = useTransiciones(solicitudId);
  const cambiarEstado = useCambiarEstado();
  const [errorEnvio, setErrorEnvio] = useState<string | null>(null);
  const [transicionSeleccionada, setTransicionSeleccionada] = useState<TransicionDisponible | undefined>(undefined);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<CambiarEstadoFormValues>({
    resolver: zodResolver(cambiarEstadoSchema),
    mode: 'onTouched',
    defaultValues: { estadoDestinoId: 0, comentario: '', requiereComentario: false },
  });

  function manejarCambioEstado(valor: string) {
    const id = Number(valor);
    const transicion = transiciones.find((item) => item.estadoDestinoId === id);
    setTransicionSeleccionada(transicion);
    setValue('estadoDestinoId', id, { shouldValidate: true });
    setValue('requiereComentario', transicion?.requiereComentario ?? false, { shouldValidate: true });
  }

  async function onSubmit(valores: CambiarEstadoFormValues) {
    setErrorEnvio(null);
    try {
      await cambiarEstado.mutateAsync({
        id: solicitudId,
        datos: { estadoDestinoId: valores.estadoDestinoId, comentario: valores.comentario || undefined },
      });
      toast.success('Estado actualizado');
      onCerrar();
    } catch (error) {
      const mensaje = error instanceof ErrorApi ? error.message : 'No se pudo cambiar el estado.';
      setErrorEnvio(mensaje);
      toast.error(mensaje);
    }
  }

  const etiquetaComentario = transicionSeleccionada?.codigo === CodigosEstadoSolicitud.RESUELTA ? 'Comentario de resolución' : 'Comentario';

  return (
    <Modal abierto titulo="Cambiar estado" onCerrar={onCerrar}>
      {isLoading ? (
        <p className="text-sm text-slate-500">Cargando…</p>
      ) : transiciones.length === 0 ? (
        <p className="text-sm text-slate-500">No hay cambios de estado disponibles para esta solicitud.</p>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-3" noValidate>
          {errorEnvio && <p className="rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorEnvio}</p>}

          <FormField etiqueta="Nuevo estado" error={errors.estadoDestinoId?.message}>
            <select
              {...register('estadoDestinoId', { valueAsNumber: true })}
              onChange={(evento) => manejarCambioEstado(evento.target.value)}
              className={inputClasses(false)}
            >
              <option value={0}>Seleccione un estado…</option>
              {transiciones.map((transicion) => (
                <option key={transicion.estadoDestinoId} value={transicion.estadoDestinoId}>
                  {transicion.nombre}
                </option>
              ))}
            </select>
          </FormField>

          <FormField
            etiqueta={`${etiquetaComentario}${transicionSeleccionada?.requiereComentario ? ' (obligatorio)' : ' (opcional)'}`}
            error={errors.comentario?.message}
          >
            <textarea rows={3} placeholder="Describa el motivo del cambio…" {...register('comentario')} />
          </FormField>

          <div className="flex justify-end gap-2">
            <Button type="button" variante="secundario" tamano="sm" onClick={onCerrar}>
              Cancelar
            </Button>
            <Button type="submit" tamano="sm" cargando={isSubmitting}>
              Confirmar
            </Button>
          </div>
        </form>
      )}
    </Modal>
  );
}
