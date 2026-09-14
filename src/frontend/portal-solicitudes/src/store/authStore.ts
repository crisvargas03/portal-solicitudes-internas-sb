import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { Usuario } from '../types';

export interface Sesion {
  usuario: Usuario;
  token: string;
  /** ISO 8601 UTC, tal como lo manda POST /api/auth/login. */
  expiraEn: string;
}

interface AuthState {
  user: Usuario | null;
  token: string | null;
  expiraEn: string | null;
  /** true mientras se revalida un token persistido contra GET /api/auth/me al arrancar. */
  validandoSesion: boolean;
  iniciarSesion: (sesion: Sesion) => void;
  actualizarUsuario: (user: Usuario) => void;
  /** Reemplaza token + expiraEn tras una renovacion (ver ADR-0019), conservando el usuario. */
  actualizarToken: (token: string, expiraEn: string) => void;
  logout: () => void;
  setValidandoSesion: (validando: boolean) => void;
}

// Persistido para que la sesion sobreviva a un refresh; isAuthenticated se deriva de user !== null
// en vez de guardarse aparte, para no tener dos fuentes de verdad. El token viaja en localStorage
// junto al usuario (ver ADR-0015): es el mismo lugar que ya se aceptaba para el rol, y sin refresh
// token real el access token no puede vivir solo en memoria sin forzar un re-login en cada recarga.
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      expiraEn: null,
      validandoSesion: true,
      iniciarSesion: ({ usuario, token, expiraEn }) => set({ user: usuario, token, expiraEn }),
      actualizarUsuario: (user) => set({ user }),
      actualizarToken: (token, expiraEn) => set({ token, expiraEn }),
      logout: () => set({ user: null, token: null, expiraEn: null }),
      setValidandoSesion: (validando) => set({ validandoSesion: validando }),
    }),
    {
      name: 'portal-solicitudes-auth',
      // validandoSesion no debe persistir: cada arranque de la app debe revalidar de nuevo.
      partialize: (state) => ({ user: state.user, token: state.token, expiraEn: state.expiraEn }),
    },
  ),
);
