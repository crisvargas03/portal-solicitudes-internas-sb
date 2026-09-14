import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { Plus, Send } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';
import { FormField } from '../ui/FormField';
import { Modal } from '../ui/Modal';
import { useCrearComentario } from '../../hooks/queries/useSolicitudes';
import { ErrorApi } from '../../lib/apiClient';
import { comentarioSchema, type ComentarioFormValues } from '../../schemas/comentarioSchema';
import type { ComentarioDetalle } from '../../types';

interface ComentariosSectionProps {
  solicitudId: number;
  comentarios: ComentarioDetalle[];
  /** Admin/Analista pueden marcar un comentario como interno (ver ADR-0023); Solicitante nunca
   * ve ese control, ni siquiera deshabilitado — tampoco recibe comentarios internos ajenos. */
  puedeComentarInterno: boolean;
}

/** Resumen + lista de comentarios de lectura; el formulario vive en un modal (ver ComentarioModal). */
export function ComentariosSection({ solicitudId, comentarios, puedeComentarInterno }: ComentariosSectionProps) {
  const [modalAbierto, setModalAbierto] = useState(false);
  const publicos = comentarios.filter((comentario) => !comentario.esInterno).length;
  const internos = comentarios.length - publicos;

  return (
    <Card
      titulo="Comentarios"
      accion={
        <Button type="button" variante="secundario" tamano="sm" icono={Plus} onClick={() => setModalAbierto(true)}>
          Comentar
        </Button>
      }
    >
      <div className="flex flex-col gap-3">
        {comentarios.length === 0 ? (
          <p className="text-sm text-slate-500">Todavía no hay comentarios.</p>
        ) : (
          <>
            <p className="text-xs text-slate-500">
              {puedeComentarInterno
                ? `${publicos} público${publicos === 1 ? '' : 's'} · ${internos} interno${internos === 1 ? '' : 's'}`
                : `${comentarios.length} comentario${comentarios.length === 1 ? '' : 's'}`}
            </p>
            <ul className="flex flex-col gap-3">
              {comentarios.map((comentario) => (
                <li key={comentario.id} className="rounded-md border border-slate-200 px-4 py-3 text-sm">
                  <div className="flex items-center justify-between gap-2">
                    <p className="text-slate-700">{comentario.texto}</p>
                    {puedeComentarInterno &&
                      (comentario.esInterno ? <Badge tono="neutral">Interno</Badge> : <Badge tono="navy">Público</Badge>)}
                  </div>
                  <p className="mt-1 text-xs text-slate-500">
                    {comentario.usuario.nombre} · {new Date(comentario.fecha).toLocaleString()}
                  </p>
                </li>
              ))}
            </ul>
          </>
        )}
      </div>

      {modalAbierto && (
        <ComentarioModal solicitudId={solicitudId} puedeComentarInterno={puedeComentarInterno} onCerrar={() => setModalAbierto(false)} />
      )}
    </Card>
  );
}

interface ComentarioModalProps {
  solicitudId: number;
  puedeComentarInterno: boolean;
  onCerrar: () => void;
}

function ComentarioModal({ solicitudId, puedeComentarInterno, onCerrar }: ComentarioModalProps) {
  const crearComentario = useCrearComentario();
  const [errorComentario, setErrorComentario] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ComentarioFormValues>({
    resolver: zodResolver(comentarioSchema),
    mode: 'onTouched',
    defaultValues: { texto: '', esInterno: false },
  });

  async function onSubmit(valores: ComentarioFormValues) {
    setErrorComentario(null);
    try {
      await crearComentario.mutateAsync({
        id: solicitudId,
        texto: valores.texto,
        esInterno: puedeComentarInterno && valores.esInterno,
      });
      toast.success('Comentario agregado');
      onCerrar();
    } catch (error) {
      const mensaje = error instanceof ErrorApi ? error.message : 'No se pudo agregar el comentario.';
      setErrorComentario(mensaje);
      toast.error(mensaje);
    }
  }

  return (
    <Modal abierto titulo="Agregar comentario" onCerrar={onCerrar}>
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-3" noValidate>
        {errorComentario && <p className="rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorComentario}</p>}
        <FormField etiqueta="Comentario" error={errors.texto?.message}>
          <textarea rows={4} placeholder="Escriba un comentario…" {...register('texto')} />
        </FormField>
        {puedeComentarInterno && (
          <label className="flex items-center gap-2 text-sm text-slate-600">
            <input type="checkbox" {...register('esInterno')} />
            Comentario interno
            <span className="text-xs text-slate-400">(no visible para el solicitante)</span>
          </label>
        )}
        <div className="flex justify-end gap-2">
          <Button type="button" variante="secundario" tamano="sm" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button type="submit" tamano="sm" icono={Send} cargando={isSubmitting}>
            Comentar
          </Button>
        </div>
      </form>
    </Modal>
  );
}
