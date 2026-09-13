import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Pencil, Plus } from 'lucide-react';
import { useActualizarUsuario, useCrearUsuario, useUsuarios } from '../hooks/queries/useUsuarios';
import { ErrorApi } from '../lib/apiClient';
import { crearUsuarioSchema, editarUsuarioSchema } from '../schemas/usuarioSchema';
import type { CrearUsuarioFormValues, EditarUsuarioFormValues } from '../schemas/usuarioSchema';
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

export function AdminUsuarios() {
  const usuarioActualId = useAuthStore((state) => state.user?.id);
  const [filtro, setFiltro] = useState<'activos' | 'todos'>('activos');
  const { data, isLoading } = useUsuarios({ soloActivos: filtro === 'activos' });
  const usuarios = data?.elementos ?? [];

  const crear = useCrearUsuario();
  const actualizar = useActualizarUsuario();
  const [editando, setEditando] = useState<UsuarioResumen | null>(null);
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  const formCrear = useForm<CrearUsuarioFormValues>({
    resolver: zodResolver(crearUsuarioSchema),
    mode: 'onTouched',
    defaultValues: { nombre: '', email: '', password: '', rol: 'Solicitante' },
  });

  const formEditar = useForm<EditarUsuarioFormValues>({
    resolver: zodResolver(editarUsuarioSchema),
    mode: 'onTouched',
    defaultValues: { nombre: '', rol: 'Solicitante', activo: true },
  });

  useEffect(() => {
    if (editando) {
      formEditar.reset({ nombre: editando.nombre, rol: editando.rol, activo: editando.activo });
    }
  }, [editando, formEditar]);

  async function onCrear(valores: CrearUsuarioFormValues) {
    setErrorGeneral(null);
    try {
      await crear.mutateAsync(valores);
      formCrear.reset({ nombre: '', email: '', password: '', rol: 'Solicitante' });
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo crear el usuario.');
    }
  }

  async function onEditar(valores: EditarUsuarioFormValues) {
    if (!editando) return;
    setErrorGeneral(null);
    try {
      await actualizar.mutateAsync({ id: editando.id, datos: valores });
      setEditando(null);
    } catch (error) {
      setErrorGeneral(error instanceof ErrorApi ? error.message : 'No se pudo actualizar el usuario.');
    }
  }

  async function alternarActivo(usuario: UsuarioResumen) {
    setErrorGeneral(null);
    try {
      await actualizar.mutateAsync({ id: usuario.id, datos: { activo: !usuario.activo } });
    } catch (error) {
      setErrorGeneral(
        error instanceof ErrorApi ? error.message : `No se pudo ${usuario.activo ? 'desactivar' : 'reactivar'} el usuario.`,
      );
    }
  }

  // Convención UX sobre la regla real del servidor (ADR-0022): un Administrador no puede
  // tocar a otro Administrador, salvo a sí mismo. El servidor rechaza esto igual si se evade la UI.
  function puedeModificar(usuario: UsuarioResumen): boolean {
    return usuario.rol !== 'Administrador' || usuario.id === usuarioActualId;
  }

  const columnas: Columna<UsuarioResumen>[] = [
    { clave: 'nombre', encabezado: 'Nombre' },
    { clave: 'email', encabezado: 'Correo' },
    { clave: 'rol', encabezado: 'Rol', render: (fila) => <Badge tono="navy">{fila.rol}</Badge> },
    {
      clave: 'activo',
      encabezado: 'Estado',
      render: (fila) => <Badge tono={fila.activo ? 'exito' : 'neutral'}>{fila.activo ? 'Activo' : 'Inactivo'}</Badge>,
    },
    {
      clave: 'acciones',
      encabezado: '',
      alineacion: 'derecha',
      render: (fila) => {
        const habilitado = puedeModificar(fila);
        return (
          <div className="flex justify-end gap-2">
            <Button variante="fantasma" tamano="sm" icono={Pencil} disabled={!habilitado} onClick={() => setEditando(fila)}>
              Editar
            </Button>
            <Button variante="secundario" tamano="sm" disabled={!habilitado} onClick={() => alternarActivo(fila)}>
              {fila.activo ? 'Desactivar' : 'Reactivar'}
            </Button>
          </div>
        );
      },
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-navy">Usuarios</h2>
        <p className="text-sm text-slate-500">Alta, edición y baja de usuarios del portal.</p>
      </div>

      {errorGeneral && <p className="rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorGeneral}</p>}

      <Card titulo={editando ? `Editar ${editando.nombre}` : 'Nuevo usuario'}>
        {editando ? (
          <form onSubmit={formEditar.handleSubmit(onEditar)} className="flex flex-wrap items-end gap-4" noValidate>
            <FormField etiqueta="Nombre" error={formEditar.formState.errors.nombre?.message} className="min-w-[220px] flex-1">
              <input type="text" {...formEditar.register('nombre')} />
            </FormField>
            <FormField etiqueta="Rol" error={formEditar.formState.errors.rol?.message} className="w-48">
              <select {...formEditar.register('rol')}>
                {ROLES.map((rol) => (
                  <option key={rol} value={rol}>
                    {rol}
                  </option>
                ))}
              </select>
            </FormField>
            <div className="flex gap-2">
              <Button type="submit" cargando={formEditar.formState.isSubmitting}>
                Guardar cambios
              </Button>
              <Button type="button" variante="secundario" onClick={() => setEditando(null)}>
                Cancelar
              </Button>
            </div>
          </form>
        ) : (
          <form onSubmit={formCrear.handleSubmit(onCrear)} className="flex flex-wrap items-end gap-4" noValidate>
            <FormField etiqueta="Nombre" error={formCrear.formState.errors.nombre?.message} className="min-w-[200px] flex-1">
              <input type="text" placeholder="Nombre completo" {...formCrear.register('nombre')} />
            </FormField>
            <FormField etiqueta="Correo" error={formCrear.formState.errors.email?.message} className="min-w-[220px] flex-1">
              <input type="email" placeholder="nombre@sb.gob.do" {...formCrear.register('email')} />
            </FormField>
            <FormField etiqueta="Contraseña" error={formCrear.formState.errors.password?.message} className="w-48">
              <input type="password" placeholder="••••••••" {...formCrear.register('password')} />
            </FormField>
            <FormField etiqueta="Rol" error={formCrear.formState.errors.rol?.message} className="w-44">
              <select {...formCrear.register('rol')}>
                {ROLES.map((rol) => (
                  <option key={rol} value={rol}>
                    {rol}
                  </option>
                ))}
              </select>
            </FormField>
            <Button type="submit" cargando={formCrear.formState.isSubmitting} icono={Plus}>
              Crear usuario
            </Button>
          </form>
        )}
      </Card>

      <SegmentedControl opciones={FILTROS} valor={filtro} onChange={(valor) => setFiltro(valor as 'activos' | 'todos')} />

      <DataTable
        columnas={columnas}
        filas={usuarios}
        obtenerClave={(fila) => fila.id}
        cargando={isLoading}
        mensajeVacio="No hay usuarios que coincidan con el filtro."
      />
    </div>
  );
}
