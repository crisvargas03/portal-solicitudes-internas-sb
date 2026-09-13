import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { Paperclip, Plus } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';
import { FormField } from '../ui/FormField';
import { Modal } from '../ui/Modal';
import { useCrearAdjunto } from '../../hooks/queries/useSolicitudes';
import { ErrorApi } from '../../lib/apiClient';
import { adjuntoSchema, type AdjuntoFormValues } from '../../schemas/solicitudSchema';
import type { AdjuntoDetalle } from '../../types';

interface EvidenciaSectionProps {
  solicitudId: number;
  adjuntos: AdjuntoDetalle[];
}

/** Referencias de evidencia (texto/URL, ver ADR-0006): lista de lectura + modal para agregar. */
export function EvidenciaSection({ solicitudId, adjuntos }: EvidenciaSectionProps) {
  const [modalAbierto, setModalAbierto] = useState(false);

  return (
    <Card
      titulo="Evidencia"
      accion={
        <Button type="button" variante="secundario" tamano="sm" icono={Plus} onClick={() => setModalAbierto(true)}>
          Agregar
        </Button>
      }
    >
      {adjuntos.length === 0 ? (
        <p className="text-sm text-slate-500">Sin referencias de evidencia todavía.</p>
      ) : (
        <div className="divide-y divide-slate-200 rounded-md border border-slate-200">
          {adjuntos.map((adjunto) => (
            <div key={adjunto.id} className="flex items-start gap-3 px-4 py-3 text-sm">
              <Paperclip size={16} className="mt-0.5 shrink-0 text-slate-400" />
              <div className="min-w-0">
                <a
                  href={adjunto.url}
                  target="_blank"
                  rel="noreferrer"
                  className="break-words font-medium text-accent-orange hover:underline"
                >
                  {adjunto.descripcion}
                </a>
                <p className="mt-0.5 text-xs text-slate-500">
                  {adjunto.usuario.nombre} · {new Date(adjunto.fecha).toLocaleString()}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}

      {modalAbierto && <EvidenciaModal solicitudId={solicitudId} onCerrar={() => setModalAbierto(false)} />}
    </Card>
  );
}

interface EvidenciaModalProps {
  solicitudId: number;
  onCerrar: () => void;
}

function EvidenciaModal({ solicitudId, onCerrar }: EvidenciaModalProps) {
  const crearAdjunto = useCrearAdjunto();
  const [errorAdjunto, setErrorAdjunto] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<AdjuntoFormValues>({
    resolver: zodResolver(adjuntoSchema),
    mode: 'onTouched',
    defaultValues: { adjuntoDescripcion: '', adjuntoUrl: '' },
  });

  async function onSubmit(valores: AdjuntoFormValues) {
    setErrorAdjunto(null);
    try {
      await crearAdjunto.mutateAsync({
        id: solicitudId,
        datos: { descripcion: valores.adjuntoDescripcion || valores.adjuntoUrl, url: valores.adjuntoUrl },
      });
      toast.success('Referencia agregada');
      onCerrar();
    } catch (error) {
      const mensaje = error instanceof ErrorApi ? error.message : 'No se pudo agregar la referencia.';
      setErrorAdjunto(mensaje);
      toast.error(mensaje);
    }
  }

  return (
    <Modal abierto titulo="Agregar evidencia" onCerrar={onCerrar}>
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-3" noValidate>
        {errorAdjunto && <p className="rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorAdjunto}</p>}
        <FormField etiqueta="Descripción (opcional)" error={errors.adjuntoDescripcion?.message}>
          <input type="text" placeholder="Ej: Captura del error" {...register('adjuntoDescripcion')} />
        </FormField>
        <FormField etiqueta="URL de evidencia" error={errors.adjuntoUrl?.message}>
          <input type="text" placeholder="https://…" {...register('adjuntoUrl')} />
        </FormField>
        <div className="flex justify-end gap-2">
          <Button type="button" variante="secundario" tamano="sm" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button type="submit" tamano="sm" icono={Paperclip} cargando={isSubmitting}>
            Agregar
          </Button>
        </div>
      </form>
    </Modal>
  );
}
