import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Pencil, Plus } from 'lucide-react';
import { useActualizarPrioridad, useCrearPrioridad, usePrioridadesTodas } from '../../hooks/queries/useCatalogosAdmin';
import { prioridadSchema } from '../../schemas/prioridadSchema';
import type { PrioridadFormValues } from '../../schemas/prioridadSchema';
import { ErrorApi } from '../../lib/apiClient';
import type { PrioridadAdmin } from '../../types';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';
import { DataTable } from '../ui/DataTable';
import type { Columna } from '../ui/DataTable';
import { FormField } from '../ui/FormField';

export function PrioridadesAdminSection() {
  const { data: prioridades = [], isLoading } = usePrioridadesTodas();
  const crear = useCrearPrioridad();
  const actualizar = useActualizarPrioridad();
  const [editando, setEditando] = useState<PrioridadAdmin | null>(null);
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<PrioridadFormValues>({
    resolver: zodResolver(prioridadSchema),
    mode: 'onTouched',
    defaultValues: { nombre: '', nivel: 1 },
  });

  useEffect(() => {
    reset({ nombre: editando?.nombre ?? '', nivel: editando?.nivel ?? 1 });
  }, [editando, reset]);

  async function onSubmit(valores: PrioridadFormValues) {
    setErrorGeneral(null);
    try {
      if (editando) {
        await actualizar.mutateAsync({ id: editando.id, datos: valores });
        setEditando(null);
      } else {
        await crear.mutateAsync(valores);
      }
      reset({ nombre: '', nivel: 1 });
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo guardar la prioridad.');
    }
  }

  async function alternarActivo(prioridad: PrioridadAdmin) {
    await actualizar.mutateAsync({ id: prioridad.id, datos: { activo: !prioridad.activo } });
  }

  const columnas: Columna<PrioridadAdmin>[] = [
    { clave: 'nombre', encabezado: 'Nombre' },
    { clave: 'nivel', encabezado: 'Nivel', ordenable: true },
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
      <Card titulo={editando ? `Editar ${editando.nombre}` : 'Nueva prioridad'}>
        {errorGeneral && <p className="mb-4 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorGeneral}</p>}
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-wrap items-end gap-4" noValidate>
          <FormField etiqueta="Nombre" error={errors.nombre?.message} className="min-w-[240px] flex-1">
            <input type="text" placeholder="Ej: Alta" {...register('nombre')} />
          </FormField>
          <FormField etiqueta="Nivel" error={errors.nivel?.message} className="w-28">
            <input type="number" min={1} {...register('nivel', { valueAsNumber: true })} />
          </FormField>
          <div className="flex gap-2">
            <Button type="submit" cargando={isSubmitting} icono={editando ? undefined : Plus}>
              {editando ? 'Guardar cambios' : 'Crear prioridad'}
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
        filas={prioridades}
        obtenerClave={(fila) => fila.id}
        cargando={isLoading}
        mensajeVacio="No hay prioridades registradas."
      />
    </div>
  );
}
