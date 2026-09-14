# ADR-0019: Renovación de sesión

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

El JWT emitido por `POST /api/auth/login` dura 60 minutos (`Jwt:MinutosExpiracion`) y, hasta [ADR-0015](ADR-0015-cliente-http-y-sesion-jwt.md), no existía ningún mecanismo de renovación: expirar significaba re-login forzado, incluso en medio de una sesión de trabajo activa. Este ADR se decidió como una fase separada y posterior, sobre una base de autenticación ya verificada — depurar un interceptor con renovación es mucho más fácil cuando ya consta que el 401 simple funciona.

Opciones consideradas: (A) re-emisión deslizante sin estado nuevo, (B) refresh token persistido con rotación, (C) B con el refresh token en cookie httpOnly.

## Decisión

**Re-emisión deslizante (opción A).** `POST /api/auth/refresh`, `[Authorize]`, sin cuerpo:

- `RenovarSesionCommandHandler` toma el id del usuario de `IUsuarioActual` (el mismo accesor que ya usa `ObtenerUsuarioActualQueryHandler` para `GET /api/auth/me`, y que el enricher de Serilog usa para loguear `UsuarioId`/`Rol`) — nunca lee claims a mano ni recibe el id por parámetro.
- Revalida que el usuario siga existiendo y `Activo`: sin esto, un usuario desactivado después de emitido su token podría seguir renovando esa sesión indefinidamente.
- Emite con el mismo `IProveedorTokens.GenerarToken` que usa el login, y devuelve el mismo `SesionDto` — **el contrato no cambia**, así que el frontend no tuvo que tocar tipos.
- Sin entidad nueva, sin migración, sin repositorio.

En el frontend, `useRenovacionSesion` programa la renovación ~10 minutos antes de `expiraEn` (mientras haya sesión) y vuelve a programar tras cada éxito. `/auth/refresh` está exenta del logout automático por 401 del interceptor de `apiClient` (igual que `/auth/login`): si la renovación falla, quien la llama cierra la sesión una sola vez, en vez de que el interceptor dispare una cascada.

## Consecuencias

- **A favor:** cero costo de esquema — ninguna migración, ninguna tabla nueva, ningún repositorio; el contrato de `SesionDto` no cambia, así que consumir el nuevo endpoint desde el frontend no rompe ningún tipo existente; una sesión de trabajo activa nunca se interrumpe por expiración mientras la pestaña permanezca abierta.
- **En contra / trade-offs:** solo renueva un token **todavía válido** — si la pestaña queda cerrada (o dormida) más de 60 minutos, no hay nada que renovar y el re-login sigue siendo forzado. No hay revocación: no existe manera de invalidar un token ya emitido antes de que expire por sí solo (por ejemplo, tras un cambio de contraseña o un logout forzado por un administrador). Como no hay un refresh token real y separado del access token, este último sigue necesitando persistirse en `localStorage` (ver ADR-0015) — no puede vivir solo en memoria sin forzar un re-login en cada recarga de página.
- **Alternativas descartadas:** (B) refresh token persistido con rotación — el camino "correcto" a mediano plazo: habilita revocación real y permite que el access token viva solo en memoria, pero exige una entidad `RefreshToken`, su configuración de EF, una migración, y que el frontend mantenga una sola promesa de refresh en vuelo con reintento de las peticiones que fallaron en 401 — coste no justificado para el alcance actual. (C) B con el refresh token en cookie httpOnly — la postura de seguridad más sólida (fuera del alcance de XSS), pero además exige CORS con `AllowCredentials`, `withCredentials` en el cliente y una postura explícita frente a CSRF; se deja como el paso siguiente el día que se decida invertir en revocación real.
