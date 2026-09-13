import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { Usuario } from '../types';

interface AuthState {
  user: Usuario | null;
  login: (user: Usuario) => void;
  logout: () => void;
}

// Persistido para que el rol sobreviva a un refresh; isAuthenticated se deriva de user !== null
// en vez de guardarse aparte, para no tener dos fuentes de verdad.
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      login: (user) => set({ user }),
      logout: () => set({ user: null }),
    }),
    { name: 'portal-solicitudes-auth' },
  ),
);
