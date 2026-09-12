import { type FormEvent, useState } from 'react';
import { useNavigate } from 'react-router';
import logo from '../assets/logo-superintendencia-de-bancos.png';
import { useAuthStore } from '../store/authStore';

export function Login() {
  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    login({ id: 1, nombre: 'Usuario de prueba', email, rol: 'Solicitante', activo: true });
    navigate('/dashboard');
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-navy px-4">
      <div className="w-full max-w-sm rounded-xl bg-white p-8 shadow-sm">
        <img src={logo} alt="Superintendencia de Bancos" className="mx-auto mb-6 h-14 w-auto" />
        <h1 className="mb-6 text-center text-lg font-semibold text-navy">
          Portal de Solicitudes Internas
        </h1>
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
