import axios, { AxiosError, type AxiosRequestConfig, type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../store/authStore';

const URL_BASE_API_POR_DEFECTO = 'http://localhost:5028/api';

/** Detalle de error dentro del sobre estandar (ver API/Common/ErrorApiDto.cs). */
export interface ErrorApiDto {
  codigo: string;
  detalle: string;
  errores?: Record<string, string[]> | null;
  traceId?: string | null;
}

interface RespuestaApi<T> {
  exito: boolean;
  datos: T | null;
  mensaje: string | null;
  error: ErrorApiDto | null;
}

/**
 * Error normalizado que ve el resto de la app: nunca el sobre `RespuestaApi` crudo.
 * `errores` permite mapear un 400 de validacion a los campos de un formulario.
 */
export class ErrorApi extends Error {
  readonly status: number | undefined;
  readonly codigo: string;
  readonly errores: Record<string, string[]> | null;

  constructor(mensaje: string, opciones: { status?: number; codigo: string; errores?: Record<string, string[]> | null }) {
    super(mensaje);
    this.name = 'ErrorApi';
    this.status = opciones.status;
    this.codigo = opciones.codigo;
    this.errores = opciones.errores ?? null;
  }
}

/**
 * Rutas que nunca deben disparar el logout automatico por 401 (ver interceptor de respuesta).
 * /auth/refresh tambien queda exenta (ver ADR-0019): si la renovacion falla, quien la llama
 * decide como cerrar la sesion una sola vez, en vez de que el interceptor dispare una cascada.
 */
const RUTAS_EXENTAS_DE_401 = ['/auth/login', '/auth/refresh'];

function esRutaExenta(url: string | undefined): boolean {
  return Boolean(url && RUTAS_EXENTAS_DE_401.some((ruta) => url.includes(ruta)));
}

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? URL_BASE_API_POR_DEFECTO,
});

// Interceptor de request: adjunta el Bearer si hay sesion. Ningun llamador arma este header a mano.
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = useAuthStore.getState().token;

  if (token && !esRutaExenta(config.url)) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }

  return config;
});

// Interceptor de response: desenvuelve el sobre en exito, normaliza el error en fallo.
// El tipo declarado de axios exige devolver un AxiosResponse; en runtime se devuelve `datos`
// a proposito (ver wrappers get/post mas abajo, que tipan lo que realmente llega).
apiClient.interceptors.response.use(
  (respuesta) => {
    const cuerpo = respuesta.data as RespuestaApi<unknown>;
    return cuerpo.datos as unknown as typeof respuesta;
  },
  (error: AxiosError<RespuestaApi<unknown>>) => {
    const status = error.response?.status;
    const cuerpo = error.response?.data;

    // Un JWT rechazado devuelve 401 con cuerpo vacio (middleware de autenticacion, no el sobre):
    // el logout automatico se decide por status, nunca por error.codigo.
    if (status === 401 && !esRutaExenta(error.config?.url)) {
      useAuthStore.getState().logout();

      const rutaActual = window.location.pathname + window.location.search;
      window.location.assign(`/login?volverA=${encodeURIComponent(rutaActual)}&sesionExpirada=1`);
    }

    if (cuerpo?.error) {
      return Promise.reject(
        new ErrorApi(cuerpo.error.detalle, { status, codigo: cuerpo.error.codigo, errores: cuerpo.error.errores }),
      );
    }

    return Promise.reject(
      new ErrorApi(error.message, { status, codigo: status === 401 ? 'Auth.NoAutorizado' : 'Error.Desconocido' }),
    );
  },
);

// El interceptor de response de arriba desenvuelve el sobre y devuelve `datos` en runtime,
// pero los metodos de axios siguen tipando su Promise como AxiosResponse<T>. Estos wrappers
// tipan lo que realmente se recibe, para que los servicios nunca necesiten un `as T` a mano.
export function get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
  return apiClient.get(url, config) as unknown as Promise<T>;
}

export function post<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
  return apiClient.post(url, data, config) as unknown as Promise<T>;
}

/** Serializa un objeto de filtros a query-string, omitiendo undefined/null/''. Reutilizable por cualquier servicio. */
export function aQueryString(filtros: Record<string, unknown>): string {
  const params = new URLSearchParams();

  for (const [clave, valor] of Object.entries(filtros)) {
    if (valor !== undefined && valor !== null && valor !== '') {
      params.set(clave, String(valor));
    }
  }

  const query = params.toString();
  return query ? `?${query}` : '';
}
