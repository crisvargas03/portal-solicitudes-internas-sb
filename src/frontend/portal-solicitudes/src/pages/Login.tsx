import { type FormEvent, useState } from 'react';
import { useNavigate } from 'react-router';
import logo from '../assets/logo-superintendencia-de-bancos.png';
import { useAuthStore } from '../store/authStore';
import type { RolUsuario } from '../types';

// TODO: eliminar al conectar POST /api/auth/login — selector temporal para poder
// probar las tres vistas por rol sin backend.
const ROLES_DEMO: { rol: RolUsuario; etiqueta: string }[] = [
  { rol: 'Administrador', etiqueta: 'Administrador' },
  { rol: 'Analista', etiqueta: 'Analista' },
  { rol: 'Solicitante', etiqueta: 'Solicitante' },
];

const USUARIO_DEMO_POR_ROL: Record<RolUsuario, { id: number; nombre: string }> = {
  Administrador: { id: 1, nombre: 'Admin Principal' },
  Analista: { id: 2, nombre: 'Carlos Díaz' },
  Solicitante: { id: 4, nombre: 'María Peña' },
};

export function Login() {
  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rol, setRol] = useState<RolUsuario>('Solicitante');

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const demo = USUARIO_DEMO_POR_ROL[rol];
    login({ id: demo.id, nombre: demo.nombre, email, rol, activo: true });
    navigate('/dashboard');
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-navy px-4">
      <div className="w-full max-w-sm rounded-xl bg-white p-8 shadow-sm">
        <img src={logo} alt="Superintendencia de Bancos" className="mx-auto mb-6 h-14 w-auto" />
        <h1 className="mb-6 text-center text-lg font-semibold text-navy">Portal de Solicitudes Internas</h1>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
            Correo institucional
            <input
              type="email"
              required
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange"
              placeholder="nombre@sb.gob.do"
            />
          </label>
          <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
            Contraseña
            <input
              type="password"
              required
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange"
              placeholder="••••••••"
            />
          </label>

          <div className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
            Rol (demo)
            <div className="flex gap-1.5">
              {ROLES_DEMO.map((opcion) => (
                <button
                  key={opcion.rol}
                  type="button"
                  onClick={() => setRol(opcion.rol)}
                  className={`flex-1 rounded-md border px-2 py-1.5 text-xs font-medium transition-colors ${
                    rol === opcion.rol
                      ? 'border-accent-orange bg-accent-orange/10 text-accent-orange'
                      : 'border-slate-300 text-slate-500 hover:border-accent-orange'
                  }`}
                >
                  {opcion.etiqueta}
                </button>
              ))}
            </div>
          </div>

          <button
            type="submit"
            className="mt-2 rounded-md bg-navy px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-accent-orange"
          >
            Iniciar sesión
          </button>
        </form>
      </div>
    </div>
  );
}
