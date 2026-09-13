import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';
import { toast } from 'sonner';
import { useAreas, usePrioridades, useTiposSolicitud } from '../hooks/queries/useCatalogos';
import { useActualizarSolicitudCompleta, useCrearAdjunto, useCrearSolicitud, useSolicitud } from '../hooks/queries/useSolicitudes';
import { useRolActual } from '../hooks/useRolActual';
import { ErrorApi } from '../lib/apiClient';
import { solicitudCrearSchema } from '../schemas/solicitudSchema';
import type { SolicitudCrearFormValues } from '../schemas/solicitudSchema';
import { Button } from '../components/ui/Button';
import { FormField } from '../components/ui/FormField';

export function SolicitudForm() {
  const { id } = useParams();
  const idNumerico = Number(id);
  const isEdit = Boolean(id);
  const navigate = useNavigate();
  const { esAdministrador } = useRolActual();
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  const { data: tiposSolicitud = [] } = useTiposSolicitud();
  const { data: areas = [] } = useAreas();
  const { data: prioridades = [] } = usePrioridades();
  const { data: solicitud } = useSolicitud(idNumerico);
  const actualizarCompleta = useActualizarSolicitudCompleta();
  const crearSolicitud = useCrearSolicitud();
  const crearAdjunto = useCrearAdjunto();

  // Solo Administrador tiene edicion completa (titulo/descripcion/tipo/prioridad/area) sin
  // restriccion de estado (ver ADR-0021); Solicitante/Analista siguen su PATCH parcial existente,
  // limitado a REGISTRADA, que esta pantalla no cubre todavia.
  const esEdicionAdmin = isEdit && esAdministrador;
  const camposHabilitados = !isEdit || esEdicionAdmin;

  // Un solo esquema para ambos modos: los campos de evidencia son opcionales y, en edición de
  // Administrador, ni se registran ni se envían (ver el fieldset condicional más abajo), así que
  // validarlos igual no afecta ese flujo — evita duplicar el tipo de formulario en una unión.
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<SolicitudCrearFormValues>({
    resolver: zodResolver(solicitudCrearSchema),
    mode: 'onTouched',
    defaultValues: {
      titulo: '',
      descripcion: '',
      tipoSolicitudId: 0,
      prioridadId: 0,
      areaId: 0,
      adjuntoDescripcion: '',
      adjuntoUrl: '',
    },
  });

  useEffect(() => {
    if (solicitud && esEdicionAdmin) {
      reset({
        titulo: solicitud.titulo,
        descripcion: solicitud.descripcion,
        tipoSolicitudId: solicitud.tipoSolicitud.id,
        prioridadId: solicitud.prioridad.id,
        areaId: solicitud.area.id,
      });
    }
  }, [solicitud, esEdicionAdmin, reset]);

  async function onSubmit(valores: SolicitudCrearFormValues) {
    setErrorGeneral(null);
    try {
      if (esEdicionAdmin) {
        await actualizarCompleta.mutateAsync({
          id: idNumerico,
          datos: {
            titulo: valores.titulo,
            descripcion: valores.descripcion,
            tipoSolicitudId: valores.tipoSolicitudId,
            prioridadId: valores.prioridadId,
            areaId: valores.areaId,
          },
        });
        toast.success('Solicitud actualizada');
        navigate(`/solicitudes/${idNumerico}`);
        return;
      }

      const creada = await crearSolicitud.mutateAsync({
        titulo: valores.titulo,
        descripcion: valores.descripcion,
        tipoSolicitudId: valores.tipoSolicitudId,
        prioridadId: valores.prioridadId,
        areaId: valores.areaId,
      });

      // La evidencia es un segundo POST, no una transaccion (ver ADR-0025): si falla, la
      // solicitud ya creada no se pierde — solo se avisa que falta agregarla desde el detalle.
      if (valores.adjuntoUrl) {
        try {
          await crearAdjunto.mutateAsync({
            id: creada.id,
            datos: { descripcion: valores.adjuntoDescripcion || valores.adjuntoUrl, url: valores.adjuntoUrl },
          });
        } catch {
          toast.warning('La solicitud se creó, pero no se pudo guardar la evidencia. Agréguela desde el detalle.');
          navigate(`/solicitudes/${creada.id}`);
          return;
        }
      }

      toast.success('Solicitud registrada');
      navigate(`/solicitudes/${creada.id}`);
    } catch (error) {
      if (error instanceof ErrorApi) {
        if (error.errores) {
          // 400 de validación: claves PascalCase del backend → campos del form (ver Login.tsx).
          for (const [campo, mensajes] of Object.entries(error.errores)) {
            const nombreCampo = (campo.charAt(0).toLowerCase() + campo.slice(1)) as keyof SolicitudCrearFormValues;
            if (mensajes[0]) {
              setError(nombreCampo, { message: mensajes[0] });
            }
          }
          return;
        }

        setErrorGeneral(error.message);
        toast.error(error.message);
        return;
      }

      const mensaje = 'No se pudo guardar la solicitud.';
      setErrorGeneral(mensaje);
      toast.error(mensaje);
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-navy">{isEdit ? 'Editar solicitud' : 'Nueva solicitud'}</h2>

      {errorGeneral && <p className="max-w-2xl rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorGeneral}</p>}

      {!esEdicionAdmin && isEdit && (
        <p className="max-w-2xl rounded-md bg-slate-100 px-3 py-2 text-xs text-slate-500">
          Su rol solo puede editar esta solicitud mientras está en estado Registrada, desde las acciones de la solicitud.
        </p>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="flex max-w-2xl flex-col gap-6" noValidate>
        <fieldset className="flex flex-col gap-4 rounded-md border border-slate-200 p-4" disabled={!camposHabilitados}>
          <legend className="px-1 text-sm font-semibold text-navy">Detalle de la solicitud</legend>

          <FormField etiqueta="Título" error={errors.titulo?.message}>
            <input type="text" placeholder="Ej: Acceso a sistema de reportes" {...register('titulo')} />
          </FormField>

          <FormField etiqueta="Descripción" error={errors.descripcion?.message}>
            <textarea rows={4} placeholder="Detalle de la solicitud" {...register('descripcion')} />
          </FormField>
        </fieldset>

        <fieldset className="grid grid-cols-1 gap-4 rounded-md border border-slate-200 p-4 sm:grid-cols-3" disabled={!camposHabilitados}>
          <legend className="px-1 text-sm font-semibold text-navy">Clasificación</legend>

          <FormField etiqueta="Tipo de solicitud" error={errors.tipoSolicitudId?.message}>
            <select {...register('tipoSolicitudId', { valueAsNumber: true })}>
              <option value={0}>Seleccione…</option>
              {tiposSolicitud.map((tipo) => (
                <option key={tipo.id} value={tipo.id}>
                  {tipo.nombre}
                </option>
              ))}
            </select>
          </FormField>

          <FormField etiqueta="Prioridad" error={errors.prioridadId?.message}>
            <select {...register('prioridadId', { valueAsNumber: true })}>
              <option value={0}>Seleccione…</option>
              {prioridades.map((prioridad) => (
                <option key={prioridad.id} value={prioridad.id}>
                  {prioridad.nombre}
                </option>
              ))}
            </select>
          </FormField>

          <FormField etiqueta="Área" error={errors.areaId?.message}>
            <select {...register('areaId', { valueAsNumber: true })}>
              <option value={0}>Seleccione…</option>
              {areas.map((area) => (
                <option key={area.id} value={area.id}>
                  {area.nombre}
                </option>
              ))}
            </select>
          </FormField>
        </fieldset>

        {/* La evidencia es un Adjunto (ver ADR-0006), no un campo de Solicitud: no existe en el
            PUT de edición completa de Administrador ni en el PATCH parcial de otros roles, por
            eso este bloque solo aparece al crear (nunca en modo edición, habilitado o no). */}
        {!isEdit && (
          <fieldset className="grid grid-cols-1 gap-4 rounded-md border border-slate-200 p-4 sm:grid-cols-2">
            <legend className="px-1 text-sm font-semibold text-navy">Evidencia (opcional)</legend>

            <FormField etiqueta="Descripción" error={errors.adjuntoDescripcion?.message}>
              <input type="text" placeholder="Ej: Captura del error" {...register('adjuntoDescripcion')} />
            </FormField>

            <FormField etiqueta="URL de evidencia" error={errors.adjuntoUrl?.message}>
              <input type="text" placeholder="https://…" {...register('adjuntoUrl')} />
            </FormField>
          </fieldset>
        )}

        {camposHabilitados && (
          <Button type="submit" cargando={isSubmitting} className="w-fit">
            {isEdit ? 'Guardar cambios' : 'Registrar solicitud'}
          </Button>
        )}
      </form>
    </div>
  );
}
