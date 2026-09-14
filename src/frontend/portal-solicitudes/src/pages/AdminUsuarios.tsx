import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Pencil, Plus } from 'lucide-react';
import {
	useActualizarUsuario,
	useCrearUsuario,
	useUsuarios,
} from '../hooks/queries/useUsuarios';
import { useConfirm } from '../hooks/useConfirm';
import { ErrorApi } from '../lib/apiClient';
import {
	crearUsuarioSchema,
	editarUsuarioSchema,
} from '../schemas/usuarioSchema';
import type {
	CrearUsuarioFormValues,
	EditarUsuarioFormValues,
} from '../schemas/usuarioSchema';
import { useAuthStore } from '../store/authStore';
import { Badge } from '../components/ui/Badge';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { DataTable } from '../components/ui/DataTable';
import type { Columna } from '../components/ui/DataTable';
import { FormField } from '../components/ui/FormField';
import { SegmentedControl } from '../components/ui/SegmentedControl';
import type { RolUsuario, UsuarioResumen } from '../types';

const ROLES: RolUsuario[] = ['Administrador', 'Analista', 'Solicitante'];
const FILTROS = [
	{ valor: 'activos', etiqueta: 'Activos' },
	{ valor: 'todos', etiqueta: 'Todos' },
];

interface UsuarioCrearFormProps {
	onGuardar: (valores: CrearUsuarioFormValues) => Promise<void>;
}

function UsuarioCrearForm({ onGuardar }: UsuarioCrearFormProps) {
	const {
		register,
		handleSubmit,
		formState: { errors, isSubmitting },
	} = useForm<CrearUsuarioFormValues>({
		resolver: zodResolver(crearUsuarioSchema),
		mode: 'onTouched',
		defaultValues: {
			nombre: '',
			email: '',
			password: '',
			rol: 'Solicitante',
		},
	});

	return (
		<form
			onSubmit={handleSubmit(onGuardar)}
			className='flex flex-wrap items-end gap-4'
			noValidate>
			<FormField
				etiqueta='Nombre'
				error={errors.nombre?.message}
				className='min-w-[200px] flex-1'>
				<input
					type='text'
					placeholder='Nombre completo'
					{...register('nombre')}
				/>
			</FormField>
			<FormField
				etiqueta='Correo'
				error={errors.email?.message}
				className='min-w-[220px] flex-1'>
				<input
					type='email'
					placeholder='nombre@sb.gob.do'
					{...register('email')}
				/>
			</FormField>
			<FormField
				etiqueta='Contraseña'
				error={errors.password?.message}
				className='w-48'>
				<input
					type='password'
					placeholder='••••••••'
					{...register('password')}
				/>
			</FormField>
			<FormField
				etiqueta='Rol'
				error={errors.rol?.message}
				className='w-44'>
				<select {...register('rol')}>
					{ROLES.map(rol => (
						<option key={rol} value={rol}>
							{rol}
						</option>
					))}
				</select>
			</FormField>
			<Button type='submit' cargando={isSubmitting} icono={Plus}>
				Crear usuario
			</Button>
		</form>
	);
}

interface UsuarioEditarFormProps {
	editando: UsuarioResumen;
	onGuardar: (valores: EditarUsuarioFormValues) => Promise<void>;
	onCancelar: () => void;
}

function UsuarioEditarForm({
	editando,
	onGuardar,
	onCancelar,
}: UsuarioEditarFormProps) {
	const {
		register,
		handleSubmit,
		formState: { errors, isSubmitting },
	} = useForm<EditarUsuarioFormValues>({
		resolver: zodResolver(editarUsuarioSchema),
		mode: 'onTouched',
		defaultValues: {
			nombre: editando.nombre,
			rol: editando.rol,
			activo: editando.activo,
		},
	});

	return (
		<form
			onSubmit={handleSubmit(onGuardar)}
			className='flex flex-wrap items-end gap-4'
			noValidate>
			<FormField
				etiqueta='Nombre'
				error={errors.nombre?.message}
				className='min-w-[220px] flex-1'>
				<input type='text' {...register('nombre')} />
			</FormField>
			<FormField
				etiqueta='Rol'
				error={errors.rol?.message}
				className='w-48'>
				<select {...register('rol')}>
					{ROLES.map(rol => (
						<option key={rol} value={rol}>
							{rol}
						</option>
					))}
				</select>
			</FormField>
			<div className='flex gap-2'>
				<Button type='submit' cargando={isSubmitting}>
					Guardar cambios
				</Button>
				<Button
					type='button'
					variante='secundario'
					onClick={onCancelar}>
					Cancelar
				</Button>
			</div>
		</form>
	);
}

export function AdminUsuarios() {
	const usuarioActualId = useAuthStore(state => state.user?.id);
	const [filtro, setFiltro] = useState<'activos' | 'todos'>('activos');
	const { data, isLoading } = useUsuarios({
		soloActivos: filtro === 'activos',
	});
	const usuarios = data?.elementos ?? [];

	const crear = useCrearUsuario();
	const actualizar = useActualizarUsuario();
	const confirmar = useConfirm();
	const [editando, setEditando] = useState<UsuarioResumen | null>(null);
	// Fuerza un remount de UsuarioCrearForm tras crear: cambiar esto cambia su `key`, así que
	// arranca de nuevo con defaultValues vacíos sin necesitar reset().
	const [formularioVersion, setFormularioVersion] = useState(0);
	const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

	async function onCrear(valores: CrearUsuarioFormValues) {
		setErrorGeneral(null);
		try {
			await crear.mutateAsync(valores);
			setFormularioVersion(version => version + 1);
		} catch (error) {
			setErrorGeneral(
				error instanceof ErrorApi
					? error.message
					: 'No se pudo crear el usuario.',
			);
		}
	}

	async function onEditar(valores: EditarUsuarioFormValues) {
		if (!editando) return;
		setErrorGeneral(null);
		const esUnoMismo = editando.id === usuarioActualId;
		const cambiaRol = valores.rol !== editando.rol;
		const guardar = () =>
			actualizar.mutateAsync({ id: editando.id, datos: valores });
		try {
			if (cambiaRol) {
				const confirmado = await confirmar({
					titulo: esUnoMismo
						? 'Cambiar tu propio rol'
						: 'Cambiar rol',
					mensaje: esUnoMismo
						? `¿Cambiar tu rol de ${editando.rol} a ${valores.rol}? Perderás de inmediato el acceso a lo que tu rol actual permite, y no podrás revertirlo tú mismo.`
						: `¿Cambiar el rol de «${editando.nombre}» de ${editando.rol} a ${valores.rol}?`,
					variante: 'peligro',
					textoConfirmar: esUnoMismo
						? 'Cambiar mi rol'
						: 'Cambiar rol',
					accion: guardar,
				});
				if (!confirmado) return;
			} else {
				await guardar();
			}
			setEditando(null);
		} catch (error) {
			setErrorGeneral(
				error instanceof ErrorApi
					? error.message
					: 'No se pudo actualizar el usuario.',
			);
		}
	}

	async function alternarActivo(usuario: UsuarioResumen) {
		setErrorGeneral(null);
		const esUnoMismo = usuario.id === usuarioActualId;
		try {
			if (!usuario.activo) {
				await actualizar.mutateAsync({
					id: usuario.id,
					datos: { activo: true },
				});
				return;
			}
			await confirmar({
				titulo: esUnoMismo
					? 'Desactivar tu propia cuenta'
					: 'Desactivar usuario',
				mensaje: esUnoMismo
					? 'Estás por desactivar tu propio usuario. Perderás el acceso al portal y no podrás volver a entrar ni deshacerlo tú mismo: otro Administrador tendrá que reactivarte.'
					: `¿Desactivar a «${usuario.nombre}»? No podrá volver a iniciar sesión. Sus solicitudes y comentarios se conservan intactos.`,
				variante: 'peligro',
				textoConfirmar: esUnoMismo
					? 'Desactivar mi cuenta'
					: 'Desactivar',
				accion: () =>
					actualizar.mutateAsync({
						id: usuario.id,
						datos: { activo: false },
					}),
			});
		} catch (error) {
			setErrorGeneral(
				error instanceof ErrorApi
					? error.message
					: `No se pudo ${usuario.activo ? 'desactivar' : 'reactivar'} el usuario.`,
			);
		}
	}

	// Convención UX sobre la regla real del servidor (ADR-0022): un Administrador no puede
	// tocar a otro Administrador, salvo a sí mismo. El servidor rechaza esto igual si se evade la UI.
	function puedeModificar(usuario: UsuarioResumen): boolean {
		return (
			usuario.rol !== 'Administrador' || usuario.id === usuarioActualId
		);
	}

	const columnas: Columna<UsuarioResumen>[] = [
		{ clave: 'nombre', encabezado: 'Nombre' },
		{ clave: 'email', encabezado: 'Correo' },
		{
			clave: 'rol',
			encabezado: 'Rol',
			render: fila => <Badge tono='navy'>{fila.rol}</Badge>,
		},
		{
			clave: 'activo',
			encabezado: 'Estado',
			render: fila => (
				<Badge tono={fila.activo ? 'exito' : 'neutral'}>
					{fila.activo ? 'Activo' : 'Inactivo'}
				</Badge>
			),
		},
		{
			clave: 'acciones',
			encabezado: '',
			alineacion: 'derecha',
			render: fila => {
				const habilitado = puedeModificar(fila);
				return (
					<div className='flex justify-end gap-2'>
						<Button
							variante='fantasma'
							tamano='sm'
							icono={Pencil}
							disabled={!habilitado}
							onClick={() => setEditando(fila)}>
							Editar
						</Button>
						<Button
							variante='secundario'
							tamano='sm'
							disabled={!habilitado}
							onClick={() => alternarActivo(fila)}>
							{fila.activo ? 'Desactivar' : 'Reactivar'}
						</Button>
					</div>
				);
			},
		},
	];

	return (
		<div className='flex flex-col gap-6'>
			<div>
				<h2 className='text-xl font-semibold text-navy'>Usuarios</h2>
				<p className='text-sm text-slate-500'>
					Alta, edición y baja de usuarios del portal.
				</p>
			</div>

			{errorGeneral && (
				<p className='rounded-md bg-danger-soft px-3 py-2 text-xs text-danger'>
					{errorGeneral}
				</p>
			)}

			<Card
				titulo={
					editando ? `Editar ${editando.nombre}` : 'Nuevo usuario'
				}>
				{editando ? (
					<UsuarioEditarForm
						key={editando.id}
						editando={editando}
						onGuardar={onEditar}
						onCancelar={() => setEditando(null)}
					/>
				) : (
					<UsuarioCrearForm
						key={formularioVersion}
						onGuardar={onCrear}
					/>
				)}
			</Card>

			<SegmentedControl
				opciones={FILTROS}
				valor={filtro}
				onChange={valor => setFiltro(valor as 'activos' | 'todos')}
			/>

			<DataTable
				columnas={columnas}
				filas={usuarios}
				obtenerClave={fila => fila.id}
				cargando={isLoading}
				mensajeVacio='No hay usuarios que coincidan con el filtro.'
			/>
		</div>
	);
}
