import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Pencil, Plus, Search } from 'lucide-react';
import {
	useActualizarEntidadGubernamental,
	useCrearEntidadGubernamental,
	useEntidadesGubernamentalesTodas,
} from '../../hooks/queries/useEntidadesGubernamentalesAdmin';
import { useConfirm } from '../../hooks/useConfirm';
import { entidadGubernamentalSchema } from '../../schemas/entidadGubernamentalSchema';
import type { EntidadGubernamentalFormValues } from '../../schemas/entidadGubernamentalSchema';
import { ErrorApi } from '../../lib/apiClient';
import type { EntidadGubernamentalAdmin } from '../../types';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';
import { DataTable } from '../ui/DataTable';
import type { Columna } from '../ui/DataTable';
import { FormField } from '../ui/FormField';

interface EntidadGubernamentalFormProps {
	editando: EntidadGubernamentalAdmin | null;
	onGuardar: (valores: EntidadGubernamentalFormValues) => Promise<void>;
	onCancelar: () => void;
}
function EntidadGubernamentalForm({
	editando,
	onGuardar,
	onCancelar,
}: EntidadGubernamentalFormProps) {
	const {
		register,
		handleSubmit,
		formState: { errors, isSubmitting },
	} = useForm<EntidadGubernamentalFormValues>({
		resolver: zodResolver(entidadGubernamentalSchema),
		mode: 'onTouched',
		defaultValues: {
			nombre: editando?.nombre ?? '',
			categoria: editando?.categoria ?? '',
			poderDelEstado: editando?.poderDelEstado ?? '',
			sector: editando?.sector ?? '',
		},
	});

	return (
		<form
			onSubmit={handleSubmit(onGuardar)}
			className='flex flex-col gap-4'
			noValidate>
			<div className='flex flex-wrap gap-4'>
				<FormField
					etiqueta='Nombre'
					error={errors.nombre?.message}
					className='min-w-[240px] flex-1'>
					<input
						type='text'
						placeholder='Ej: Archivo General de la Nación'
						{...register('nombre')}
					/>
				</FormField>
				<FormField
					etiqueta='Categoría'
					error={errors.categoria?.message}
					className='min-w-[200px] flex-1'>
					<input
						type='text'
						placeholder='Ej: Organismo Descentralizado'
						{...register('categoria')}
					/>
				</FormField>
			</div>
			<div className='flex flex-wrap gap-4'>
				<FormField
					etiqueta='Poder del Estado'
					error={errors.poderDelEstado?.message}
					className='min-w-[200px] flex-1'>
					<input
						type='text'
						placeholder='Ej: Poder Ejecutivo'
						{...register('poderDelEstado')}
					/>
				</FormField>
				<FormField
					etiqueta='Sector'
					error={errors.sector?.message}
					className='min-w-[200px] flex-1'>
					<input
						type='text'
						placeholder='Ej: Presidencia'
						{...register('sector')}
					/>
				</FormField>
			</div>
			<div className='flex gap-2'>
				<Button
					type='submit'
					cargando={isSubmitting}
					icono={editando ? undefined : Plus}
					className='w-fit'>
					{editando ? 'Guardar cambios' : 'Crear entidad'}
				</Button>
				{editando && (
					<Button
						type='button'
						variante='secundario'
						onClick={onCancelar}>
						Cancelar
					</Button>
				)}
			</div>
		</form>
	);
}

// Catalogo grande (~180 registros, ver ADR-0034): a diferencia de Areas/Prioridades/
// TiposSolicitud, aqui conviene un filtro de texto en cliente para encontrar una entidad.
export function EntidadesGubernamentalesAdminSection() {
	const { data: entidades = [], isLoading } =
		useEntidadesGubernamentalesTodas();
	const crear = useCrearEntidadGubernamental();
	const actualizar = useActualizarEntidadGubernamental();
	const confirmar = useConfirm();
	const [editando, setEditando] = useState<EntidadGubernamentalAdmin | null>(
		null,
	);
	const [formularioVersion, setFormularioVersion] = useState(0);
	const [errorGeneral, setErrorGeneral] = useState<string | null>(null);
	const [busqueda, setBusqueda] = useState('');

	const entidadesFiltradas = useMemo(() => {
		const texto = busqueda.trim().toLowerCase();
		if (!texto) return entidades;
		return entidades.filter(entidad =>
			[
				entidad.nombre,
				entidad.categoria,
				entidad.poderDelEstado,
				entidad.sector,
			].some(campo => campo.toLowerCase().includes(texto)),
		);
	}, [entidades, busqueda]);

	async function onSubmit(valores: EntidadGubernamentalFormValues) {
		setErrorGeneral(null);
		try {
			if (editando) {
				await actualizar.mutateAsync({
					id: editando.id,
					datos: valores,
				});
				setEditando(null);
			} else {
				await crear.mutateAsync(valores);
				setFormularioVersion(version => version + 1);
			}
		} catch (error) {
			setErrorGeneral(
				error instanceof ErrorApi
					? error.message
					: 'No se pudo guardar la entidad gubernamental.',
			);
		}
	}

	async function alternarActivo(entidad: EntidadGubernamentalAdmin) {
		setErrorGeneral(null);
		try {
			if (!entidad.activo) {
				await actualizar.mutateAsync({
					id: entidad.id,
					datos: { activo: true },
				});
				return;
			}
			await confirmar({
				titulo: 'Desactivar entidad gubernamental',
				mensaje: `¿Desactivar «${entidad.nombre}»? Dejará de aparecer como referencia disponible.`,
				variante: 'peligro',
				textoConfirmar: 'Desactivar',
				accion: () =>
					actualizar.mutateAsync({
						id: entidad.id,
						datos: { activo: false },
					}),
			});
		} catch (error) {
			setErrorGeneral(
				error instanceof ErrorApi
					? error.message
					: 'No se pudo actualizar la entidad gubernamental.',
			);
		}
	}

	const columnas: Columna<EntidadGubernamentalAdmin>[] = [
		{ clave: 'nombre', encabezado: 'Nombre' },
		{ clave: 'categoria', encabezado: 'Categoría' },
		{ clave: 'poderDelEstado', encabezado: 'Poder del Estado' },
		{ clave: 'sector', encabezado: 'Sector' },
		{
			clave: 'activo',
			encabezado: 'Estado',
			render: fila => (
				<Badge tono={fila.activo ? 'exito' : 'neutral'}>
					{fila.activo ? 'Activa' : 'Inactiva'}
				</Badge>
			),
		},
		{
			clave: 'acciones',
			encabezado: '',
			alineacion: 'derecha',
			render: fila => (
				<div className='flex justify-end gap-2'>
					<Button
						variante='fantasma'
						tamano='sm'
						icono={Pencil}
						onClick={() => setEditando(fila)}>
						Editar
					</Button>
					<Button
						variante='secundario'
						tamano='sm'
						onClick={() => alternarActivo(fila)}>
						{fila.activo ? 'Desactivar' : 'Activar'}
					</Button>
				</div>
			),
		},
	];

	return (
		<div className='flex flex-col gap-6'>
			<Card
				titulo={
					editando
						? `Editar ${editando.nombre}`
						: 'Nueva entidad gubernamental'
				}>
				{errorGeneral && (
					<p className='mb-4 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger'>
						{errorGeneral}
					</p>
				)}
				<EntidadGubernamentalForm
					key={editando?.id ?? `nuevo-${formularioVersion}`}
					editando={editando}
					onGuardar={onSubmit}
					onCancelar={() => setEditando(null)}
				/>
			</Card>

			<FormField etiqueta='Buscar' className='max-w-sm'>
				<div className='relative'>
					<Search
						size={16}
						className='pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400'
					/>
					<input
						type='text'
						placeholder='Nombre, categoría, poder del Estado o sector'
						value={busqueda}
						onChange={evento => setBusqueda(evento.target.value)}
						className='pl-9'
					/>
				</div>
			</FormField>

			<DataTable
				key={busqueda}
				columnas={columnas}
				filas={entidadesFiltradas}
				obtenerClave={fila => fila.id}
				cargando={isLoading}
				mensajeVacio={
					busqueda
						? 'No hay entidades que coincidan con la búsqueda.'
						: 'No hay entidades gubernamentales registradas.'
				}
			/>
		</div>
	);
}
