import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Pencil, Plus } from 'lucide-react';
import { useActualizarArea, useAreasTodas, useCrearArea } from '../../hooks/queries/useCatalogosAdmin';
import { useConfirm } from '../../hooks/useConfirm';
import { areaSchema } from '../../schemas/areaSchema';
import type { AreaFormValues } from '../../schemas/areaSchema';
import { ErrorApi } from '../../lib/apiClient';
import type { AreaAdmin } from '../../types';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';
import { DataTable } from '../ui/DataTable';
import type { Columna } from '../ui/DataTable';
import { FormField } from '../ui/FormField';

export function AreasAdminSection() {
  const { data: areas = [], isLoading } = useAreasTodas();
  const crear = useCrearArea();
  const actualizar = useActualizarArea();
  const confirmar = useConfirm();
  const [editando, setEditando] = useState<AreaAdmin | null>(null);
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AreaFormValues>({ resolver: zodResolver(areaSchema), mode: 'onTouched', defaultValues: { nombre: '' } });

  useEffect(() => {
    reset({ nombre: editando?.nombre ?? '' });
  }, [editando, reset]);

  async function onSubmit(valores: AreaFormValues) {
    setErrorGeneral(null);
    try {
      if (editando) {
        await actualizar.mutateAsync({ id: editando.id, datos: valores });
        setEditando(null);
      } else {
        await crear.mutateAsync(valores);
      }
      reset({ nombre: '' });
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo guardar el área.');
    }
  }

  async function alternarActivo(area: AreaAdmin) {
    setErrorGeneral(null);
    try {
      if (!area.activo) {
        await actualizar.mutateAsync({ id: area.id, datos: { activo: true } });
        return;
      }
      await confirmar({
        titulo: 'Desactivar área',
        mensaje: `¿Desactivar el área «${area.nombre}»? Dejará de aparecer al crear o editar solicitudes. Las solicitudes existentes no se verán afectadas.`,
        variante: 'peligro',
        textoConfirmar: 'Desactivar',
        accion: () => actualizar.mutateAsync({ id: area.id, datos: { activo: false } }),
      });
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo actualizar el área.');
    }
  }

  const columnas: Columna<AreaAdmin>[] = [
    { clave: 'nombre', encabezado: 'Nombre' },
    {
      clave: 'activo',
      encabezado: 'Estado',
      render: (fila) => <Badge tono={fila.activo ? 'exito' : 'neutral'}>{fila.activo ? 'Activa' : 'Inactiva'}</Badge>,
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
      <Card titulo={editando ? `Editar ${editando.nombre}` : 'Nueva área'}>
        {errorGeneral && <p className="mb-4 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorGeneral}</p>}
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-wrap items-end gap-4" noValidate>
          <FormField etiqueta="Nombre" error={errors.nombre?.message} className="min-w-[240px] flex-1">
            <input type="text" placeholder="Ej: Tecnología" {...register('nombre')} />
          </FormField>
          <div className="flex gap-2">
            <Button type="submit" cargando={isSubmitting} icono={editando ? undefined : Plus}>
              {editando ? 'Guardar cambios' : 'Crear área'}
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
        filas={areas}
        obtenerClave={(fila) => fila.id}
        cargando={isLoading}
        mensajeVacio="No hay áreas registradas."
      />
    </div>
  );
}
