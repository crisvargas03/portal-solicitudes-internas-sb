# ADR-0015: Cliente HTTP y manejo de sesión JWT

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

El frontend estaba 100% desconectado del backend: `services/solicitudService.ts` operaba sobre mocks, no existía cliente HTTP, y `pages/Login.tsx` era un selector de rol de demostración que ignoraba email/password. Había que conectar `POST /api/auth/login` real, adjuntar el JWT resultante en cada llamada subsiguiente, y decidir qué hacer cuando ese token expira o es rechazado.

Puntos que el requerimiento dejaba abiertos:

- Dónde vive el token (localStorage, memoria, sessionStorage).
- Con qué librería/patrón se construye el cliente HTTP y su interceptor.
- Qué hace la app cuando una llamada responde 401.

El backend devuelve JWT con 60 minutos de expiración (`Jwt:MinutosExpiracion`) y, al momento de esta decisión, no existe endpoint de renovación (ver [ADR-0019](ADR-0019-renovacion-de-sesion.md), añadido después).

## Decisión

**axios + interceptors.** Un cliente único en `src/lib/apiClient.ts`:

- El interceptor de *request* adjunta `Authorization: Bearer <token>` leyendo `useAuthStore.getState()` (fuera de React, nunca un hook), excepto en `/auth/login`.
- El interceptor de *response*, en éxito, desenvuelve el sobre `RespuestaApi<T>` (ver ADR-0010) y devuelve `datos` directamente — el resto de la app nunca ve el sobre crudo.
- En fallo, normaliza a una clase `ErrorApi` con `codigo`, `detalle`, `errores` y `status`, para que un formulario pueda mapear `errores` (400 de validación) a sus campos.
- Si el `status` es 401 y la petición no es `/auth/login` (ni, tras ADR-0019, `/auth/refresh`), limpia la sesión del store y redirige con `window.location.assign('/login?volverA=<ruta>&sesionExpirada=1')`. Un JWT rechazado por el middleware de autenticación devuelve 401 con **cuerpo vacío**, así que esta decisión se toma por `status`, nunca por `error.codigo`.

**Token en `localStorage`**, dentro del `authStore` de Zustand que ya usaba `persist` (clave `portal-solicitudes-auth`) para el usuario. Al arrancar la app (`useRevalidarSesion`, invocado una vez desde `App.tsx`), si hay un token persistido se revalida contra `GET /api/auth/me` antes de que `RutaProtegida` decida — evita que un token vencido muestre la app un instante antes de expulsar al usuario.

## Consecuencias

- **A favor:** un solo punto de verdad para adjuntar el token y para reaccionar a un 401, sin que ningún componente arme headers a mano; el `ErrorApi` normalizado hace que un formulario mapee errores de validación sin conocer la forma del sobre; la sesión sobrevive a un refresh de página.
- **En contra / trade-offs:** `localStorage` es legible por cualquier XSS en la página — la mitigación honesta (cookie httpOnly) es un cambio de backend que no se justificaba solo para esta conexión. Como la renovación decidida en ADR-0019 es una re-emisión deslizante (no un refresh token real), el access token sigue necesitando persistirse; con un refresh token de verdad podría vivir solo en memoria y reconstruirse en cada arranque, reduciendo esa ventana de exposición.
- **Alternativas descartadas:** `sessionStorage` (cierra sesión al cerrar la pestaña, mala experiencia de demo) y memoria-only (fuerza re-login en cada refresh sin que exista todavía un refresh token que lo compense).
