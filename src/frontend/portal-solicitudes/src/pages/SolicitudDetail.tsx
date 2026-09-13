import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { ArrowLeft, FileSearch, Paperclip, Pencil, Send } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { Link, useParams } from 'react-router';
import { toast } from 'sonner';
import { PriorityBadge } from '../components/solicitudes/PriorityBadge';
import { StatusBadge } from '../components/solicitudes/StatusBadge';
import { VencimientoIndicator } from '../components/solicitudes/VencimientoIndicator';
import { Badge } from '../components/ui/Badge';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { EmptyState } from '../components/ui/EmptyState';
import { FormField } from '../components/ui/FormField';
import { inputClasses } from '../components/ui/inputClasses';
import { useAnalistas } from '../hooks/queries/useUsuarios';
import { useCambiarAsignacion, useCrearAdjunto, useCrearComentario, useSolicitud } from '../hooks/queries/useSolicitudes';
import { useRolActual } from '../hooks/useRolActual';
import { ErrorApi } from '../lib/apiClient';
import { adjuntoSchema, type AdjuntoFormValues } from '../schemas/solicitudSchema';
import { comentarioSchema, type ComentarioFormValues } from '../schemas/comentarioSchema';
import type { AdjuntoDetalle, ComentarioDetalle } from '../types';

export function SolicitudDetail() {
  const { id } = useParams();
  const idNumerico = Number(id);
  const { esAdministrador } = useRolActual();
  const { data: solicitud, isLoading, isError } = useSolicitud(idNumerico);
  const { data: analistas = [] } = useAnalistas();
  const cambiarAsignacion = useCambiarAsignacion();
  const [analistaSeleccionado, setAnalistaSeleccionado] = useState('');
  const [errorAsignacion, setErrorAsignacion] = useState<string | null>(null);

  async function asignar() {
    setErrorAsignacion(null);
    try {
      await cambiarAsignacion.mutateAsync({
        id: idNumerico,
        usuarioAsignadoId: analistaSeleccionado ? Number(analistaSeleccionado) : null,
      });
    } catch (error) {
      setErrorAsignacion(error instanceof ErrorApi ? error.message : 'No se pudo asignar la solicitud.');
    }
  }

  if (isLoading) {
    return <p className="text-sm text-slate-500">Cargando…</p>;
  }

  if (isError || !solicitud) {
    return (
      <EmptyState
        icono={FileSearch}
        titulo="La solicitud no existe"
        descripcion="Verifique el enlace o vuelva al listado de solicitudes."
        accion={
          <Link to="/solicitudes" className="text-sm text-accent-orange hover:underline">
            Volver a solicitudes
          </Link>
        }
      />
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <Link to="/solicitudes" className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-accent-orange">
          <ArrowLeft size={16} />
          Volver a solicitudes
        </Link>
        {esAdministrador && (
          <Link to={`/solicitudes/${id}/editar`} className="flex items-center gap-1.5 text-sm text-accent-orange hover:underline">
            <Pencil size={14} />
            Editar solicitud
          </Link>
        )}
      </div>

      <div>
        <p className="text-sm text-slate-500">{solicitud.codigo}</p>
        <h2 className="text-xl font-semibold text-navy">{solicitud.titulo}</h2>
        <p className="mt-2 max-w-2xl text-sm text-slate-600">{solicitud.descripcion}</p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div>
          <p className="text-xs text-slate-500">Estado</p>
          <div className="mt-1">
            <StatusBadge estado={solicitud.estado} />
          </div>
        </div>
        <div>
          <p className="text-xs text-slate-500">Prioridad</p>
          <div className="mt-1">
            <PriorityBadge prioridad={solicitud.prioridad} />
          </div>
        </div>
        <Field label="Área" value={solicitud.area.nombre} />
        <Field label="Responsable" value={solicitud.asignado?.nombre ?? 'Sin asignar'} />
      </div>

      <div className="flex flex-wrap gap-4">
        <Field label="Solicitante" value={solicitud.solicitante.nombre} />
        <Field label="Tipo de solicitud" value={solicitud.tipoSolicitud.nombre} />
        <div>
          <p className="text-xs text-slate-500">Vencimiento</p>
          <div className="mt-1">
            <VencimientoIndicator fechaCompromiso={solicitud.fechaCompromiso} estaVencida={solicitud.estaVencida} />
          </div>
        </div>
      </div>

      {esAdministrador && (
        <section className="max-w-md rounded-md border border-slate-200 p-4">
          <h3 className="mb-3 text-sm font-semibold text-navy">Asignar responsable</h3>
          {errorAsignacion && <p className="mb-3 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorAsignacion}</p>}
          <div className="flex items-end gap-3">
            <label className="flex flex-1 flex-col gap-1.5 text-sm font-medium text-slate-700">
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
            <Button type="button" cargando={cambiarAsignacion.isPending} onClick={asignar}>
              Asignar
            </Button>
          </div>
        </section>
      )}

      <section>
        <h3 className="mb-3 text-sm font-semibold text-navy">Historial de estados</h3>
        <ul className="flex flex-col divide-y divide-slate-200 rounded-md border border-slate-200">
          {solicitud.historial.map((entry) => (
            <li key={entry.id} className="flex items-center justify-between px-4 py-3 text-sm">
              <span className="font-medium text-navy">{entry.estadoNuevo.nombre}</span>
              <span className="text-slate-500">
                {entry.usuario.nombre} · {new Date(entry.fecha).toLocaleDateString()}
              </span>
            </li>
          ))}
        </ul>
      </section>

      <ComentariosSection solicitudId={idNumerico} comentarios={solicitud.comentarios} />

      <AdjuntosSection solicitudId={idNumerico} adjuntos={solicitud.adjuntos} />
    </div>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs text-slate-500">{label}</p>
      <p className="text-sm font-medium text-navy">{value}</p>
    </div>
  );
}

interface ComentariosSectionProps {
  solicitudId: number;
  comentarios: ComentarioDetalle[];
}

/** Solo Admin/Analista pueden recibir comentarios `esInterno` — un Solicitante nunca los ve
 * en la respuesta del servidor (ver ADR-0023), así que no hace falta filtrar aquí. */
function ComentariosSection({ solicitudId, comentarios }: ComentariosSectionProps) {
  const crearComentario = useCrearComentario();
  const [errorComentario, setErrorComentario] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ComentarioFormValues>({
    resolver: zodResolver(comentarioSchema),
    mode: 'onTouched',
    defaultValues: { texto: '' },
  });

  async function onSubmit(valores: ComentarioFormValues) {
    setErrorComentario(null);
    try {
      await crearComentario.mutateAsync({ id: solicitudId, texto: valores.texto });
      reset({ texto: '' });
      toast.success('Comentario agregado');
    } catch (error) {
      const mensaje = error instanceof ErrorApi ? error.message : 'No se pudo agregar el comentario.';
      setErrorComentario(mensaje);
      toast.error(mensaje);
    }
  }

  return (
    <Card titulo="Comentarios">
      <div className="flex flex-col gap-3">
        {comentarios.length === 0 && <p className="text-sm text-slate-500">Todavía no hay comentarios.</p>}
        <ul className="flex flex-col gap-3">
          {comentarios.map((comentario) => (
            <li key={comentario.id} className="rounded-md border border-slate-200 px-4 py-3 text-sm">
              <div className="flex items-center justify-between gap-2">
                <p className="text-slate-700">{comentario.texto}</p>
                {comentario.esInterno && <Badge tono="advertencia">Interno</Badge>}
              </div>
              <p className="mt-1 text-xs text-slate-500">
                {comentario.usuario.nombre} · {new Date(comentario.fecha).toLocaleString()}
              </p>
            </li>
          ))}
        </ul>

        {errorComentario && <p className="rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorComentario}</p>}

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-2" noValidate>
          <FormField etiqueta="Agregar comentario" error={errors.texto?.message}>
            <textarea rows={3} placeholder="Escriba un comentario…" {...register('texto')} />
          </FormField>
          <Button type="submit" tamano="sm" icono={Send} cargando={isSubmitting} className="w-fit">
            Comentar
          </Button>
        </form>
      </div>
    </Card>
  );
}

interface AdjuntosSectionProps {
  solicitudId: number;
  adjuntos: AdjuntoDetalle[];
}

function AdjuntosSection({ solicitudId, adjuntos }: AdjuntosSectionProps) {
  const crearAdjunto = useCrearAdjunto();
  const [errorAdjunto, setErrorAdjunto] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
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
      reset({ adjuntoDescripcion: '', adjuntoUrl: '' });
      toast.success('Referencia agregada');
    } catch (error) {
      const mensaje = error instanceof ErrorApi ? error.message : 'No se pudo agregar la referencia.';
      setErrorAdjunto(mensaje);
      toast.error(mensaje);
    }
  }

  return (
    <Card titulo="Evidencia">
      <div className="flex flex-col gap-3">
        {adjuntos.length === 0 && <p className="text-sm text-slate-500">Sin referencias de evidencia todavía.</p>}
        <ul className="flex flex-col gap-3">
          {adjuntos.map((adjunto) => (
            <li key={adjunto.id} className="flex items-start gap-2 rounded-md border border-slate-200 px-4 py-3 text-sm">
              <Paperclip size={16} className="mt-0.5 shrink-0 text-slate-400" />
              <div>
                <a href={adjunto.url} target="_blank" rel="noreferrer" className="font-medium text-accent-orange hover:underline">
                  {adjunto.descripcion}
                </a>
                <p className="mt-1 text-xs text-slate-500">
                  {adjunto.usuario.nombre} · {new Date(adjunto.fecha).toLocaleString()}
                </p>
              </div>
            </li>
          ))}
        </ul>

        {errorAdjunto && <p className="rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorAdjunto}</p>}

        <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-1 gap-3 sm:grid-cols-2" noValidate>
          <FormField etiqueta="Descripción (opcional)" error={errors.adjuntoDescripcion?.message}>
            <input type="text" placeholder="Ej: Captura del error" {...register('adjuntoDescripcion')} />
          </FormField>
          <FormField etiqueta="URL de evidencia" error={errors.adjuntoUrl?.message}>
            <input type="text" placeholder="https://…" {...register('adjuntoUrl')} />
          </FormField>
          <Button type="submit" tamano="sm" icono={Paperclip} cargando={isSubmitting} className="w-fit sm:col-span-2">
            Agregar referencia
          </Button>
        </form>
      </div>
    </Card>
  );
}
