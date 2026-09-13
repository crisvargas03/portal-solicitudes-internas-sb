import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';
import { useAreas, usePrioridades, useTiposSolicitud } from '../hooks/queries/useCatalogos';
import { useActualizarSolicitudCompleta, useSolicitud } from '../hooks/queries/useSolicitudes';
import { useRolActual } from '../hooks/useRolActual';
import { ErrorApi } from '../lib/apiClient';
import { solicitudAdminSchema } from '../schemas/solicitudSchema';
import type { SolicitudAdminFormValues } from '../schemas/solicitudSchema';
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

  // Solo Administrador tiene edicion completa (titulo/descripcion/tipo/prioridad/area) sin
  // restriccion de estado (ver ADR-0021); Solicitante/Analista siguen su PATCH parcial existente,
  // limitado a REGISTRADA, que esta pantalla no cubre todavia.
  const esEdicionAdmin = isEdit && esAdministrador;
  // Alta de solicitud: sin endpoint real conectado aun en esta pantalla: los campos quedan
  // habilitados (como el mock original) pero el submit no hace nada todavia.
  const camposHabilitados = !isEdit || esEdicionAdmin;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SolicitudAdminFormValues>({
    resolver: zodResolver(solicitudAdminSchema),
    mode: 'onTouched',
    defaultValues: { titulo: '', descripcion: '', tipoSolicitudId: 0, prioridadId: 0, areaId: 0 },
  });

  useEffect(() => {
    if (solicitud) {
      reset({
        titulo: solicitud.titulo,
        descripcion: solicitud.descripcion,
        tipoSolicitudId: solicitud.tipoSolicitudId,
        prioridadId: solicitud.prioridadId,
        areaId: solicitud.areaId,
      });
    }
  }, [solicitud, reset]);

  async function onSubmit(valores: SolicitudAdminFormValues) {
    setErrorGeneral(null);
    try {
      await actualizarCompleta.mutateAsync({ id: idNumerico, datos: valores });
      navigate(`/solicitudes/${idNumerico}`);
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo guardar la solicitud.');
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

      <form
        onSubmit={esEdicionAdmin ? handleSubmit(onSubmit) : (event) => event.preventDefault()}
        className="grid max-w-2xl grid-cols-1 gap-4 sm:grid-cols-2"
        noValidate
      >
        <FormField etiqueta="Título" error={errors.titulo?.message} className="sm:col-span-2">
          <input
            type="text"
            placeholder="Ej: Acceso a sistema de reportes"
            disabled={!camposHabilitados}
            {...register('titulo')}
          />
        </FormField>

        <FormField etiqueta="Descripción" error={errors.descripcion?.message} className="sm:col-span-2">
          <textarea rows={4} placeholder="Detalle de la solicitud" disabled={!camposHabilitados} {...register('descripcion')} />
        </FormField>

        <FormField etiqueta="Tipo de solicitud" error={errors.tipoSolicitudId?.message}>
          <select disabled={!camposHabilitados} {...register('tipoSolicitudId', { valueAsNumber: true })}>
            <option value={0}>Seleccione…</option>
            {tiposSolicitud.map((tipo) => (
              <option key={tipo.id} value={tipo.id}>
                {tipo.nombre}
              </option>
            ))}
          </select>
        </FormField>

        <FormField etiqueta="Área" error={errors.areaId?.message}>
          <select disabled={!camposHabilitados} {...register('areaId', { valueAsNumber: true })}>
            <option value={0}>Seleccione…</option>
            {areas.map((area) => (
              <option key={area.id} value={area.id}>
                {area.nombre}
              </option>
            ))}
          </select>
        </FormField>

        <FormField etiqueta="Prioridad" error={errors.prioridadId?.message}>
          <select disabled={!camposHabilitados} {...register('prioridadId', { valueAsNumber: true })}>
            <option value={0}>Seleccione…</option>
            {prioridades.map((prioridad) => (
              <option key={prioridad.id} value={prioridad.id}>
                {prioridad.nombre}
              </option>
            ))}
          </select>
        </FormField>

        {camposHabilitados && (
          <Button type="submit" cargando={isSubmitting} className="mt-2 w-fit sm:col-span-2">
            {isEdit ? 'Guardar cambios' : 'Registrar solicitud'}
          </Button>
        )}
      </form>
    </div>
  );
}
