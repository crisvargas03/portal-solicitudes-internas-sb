import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Pencil, Plus } from 'lucide-react';
import {
  useActualizarTipoSolicitud,
  useCrearTipoSolicitud,
  useTiposSolicitudTodos,
} from '../../hooks/queries/useCatalogosAdmin';
import { useConfirm } from '../../hooks/useConfirm';
import { tipoSolicitudSchema } from '../../schemas/tipoSolicitudSchema';
import type { TipoSolicitudFormValues } from '../../schemas/tipoSolicitudSchema';
import { ErrorApi } from '../../lib/apiClient';
import type { TipoSolicitudAdmin } from '../../types';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';
import { DataTable } from '../ui/DataTable';
import type { Columna } from '../ui/DataTable';
import { FormField } from '../ui/FormField';

export function TiposSolicitudAdminSection() {
  const { data: tipos = [], isLoading } = useTiposSolicitudTodos();
  const crear = useCrearTipoSolicitud();
  const actualizar = useActualizarTipoSolicitud();
  const confirmar = useConfirm();
  const [editando, setEditando] = useState<TipoSolicitudAdmin | null>(null);
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<TipoSolicitudFormValues>({
    resolver: zodResolver(tipoSolicitudSchema),
    mode: 'onTouched',
    defaultValues: { nombre: '', descripcion: '' },
  });

  useEffect(() => {
    reset({ nombre: editando?.nombre ?? '', descripcion: editando?.descripcion ?? '' });
  }, [editando, reset]);

  async function onSubmit(valores: TipoSolicitudFormValues) {
    setErrorGeneral(null);
    try {
      const datos = { nombre: valores.nombre, descripcion: valores.descripcion || null };
      if (editando) {
        await actualizar.mutateAsync({ id: editando.id, datos });
        setEditando(null);
      } else {
        await crear.mutateAsync(datos);
      }
      reset({ nombre: '', descripcion: '' });
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo guardar el tipo de solicitud.');
    }
  }

  async function alternarActivo(tipo: TipoSolicitudAdmin) {
    setErrorGeneral(null);
    try {
      if (!tipo.activo) {
        await actualizar.mutateAsync({ id: tipo.id, datos: { activo: true } });
        return;
      }
      await confirmar({
        titulo: 'Desactivar tipo de solicitud',
        mensaje: `¿Desactivar el tipo «${tipo.nombre}»? Dejará de aparecer al crear o editar solicitudes. Las solicitudes existentes no se verán afectadas.`,
        variante: 'peligro',
        textoConfirmar: 'Desactivar',
        accion: () => actualizar.mutateAsync({ id: tipo.id, datos: { activo: false } }),
      });
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo actualizar el tipo de solicitud.');
    }
  }

  const columnas: Columna<TipoSolicitudAdmin>[] = [
    { clave: 'nombre', encabezado: 'Nombre' },
    { clave: 'descripcion', encabezado: 'Descripción', render: (fila) => fila.descripcion ?? '—' },
    {
      clave: 'activo',
      encabezado: 'Estado',
      render: (fila) => <Badge tono={fila.activo ? 'exito' : 'neutral'}>{fila.activo ? 'Activo' : 'Inactivo'}</Badge>,
    },
    {
      clave: 'acciones',
      encabezado: '',
      alineacion: 'derecha',
      render: (fila) => (
        <div className="flex justify-end gap-2">
          <Button variante="fantasma" tamano="sm" icono={Pencil} onClick={() => setEditando(fila)}>
            Editar
          </Button>
          <Button variante="secundario" tamano="sm" onClick={() => alternarActivo(fila)}>
            {fila.activo ? 'Desactivar' : 'Activar'}
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      <Card titulo={editando ? `Editar ${editando.nombre}` : 'Nuevo tipo de solicitud'}>
        {errorGeneral && <p className="mb-4 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorGeneral}</p>}
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4" noValidate>
          <div className="flex flex-wrap gap-4">
            <FormField etiqueta="Nombre" error={errors.nombre?.message} className="min-w-[240px] flex-1">
              <input type="text" placeholder="Ej: Soporte técnico" {...register('nombre')} />
            </FormField>
          </div>
          <FormField etiqueta="Descripción (opcional)" error={errors.descripcion?.message}>
            <textarea rows={2} placeholder="Detalle del tipo de solicitud" {...register('descripcion')} />
          </FormField>
          <div className="flex gap-2">
            <Button type="submit" cargando={isSubmitting} icono={editando ? undefined : Plus} className="w-fit">
              {editando ? 'Guardar cambios' : 'Crear tipo'}
            </Button>
            {editando && (
              <Button type="button" variante="secundario" onClick={() => setEditando(null)}>
                Cancelar
              </Button>
            )}
          </div>
        </form>
      </Card>

      <DataTable
        columnas={columnas}
        filas={tipos}
        obtenerClave={(fila) => fila.id}
        cargando={isLoading}
        mensajeVacio="No hay tipos de solicitud registrados."
      />
    </div>
  );
}
