# ADR-0016: Protección de rutas en el frontend

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

Antes de este cambio no existía ningún guard: `/dashboard`, `/solicitudes`, etc. renderizaban con `user === null`, y `RoleSwitch` (el despachador que elige qué vista de rol mostrar) caía al branch **Solicitante** cuando no había sesión — un usuario sin login veía la app como si fuera un Solicitante en vez de ser enviado al login. Con el JWT real conectado (ver [ADR-0015](ADR-0015-cliente-http-y-sesion-jwt.md)) esto deja de ser aceptable.

También estaba abierto qué rol puede llegar a qué ruta por URL directa, más allá de lo que el sidebar (`config/navigation.ts`) ya oculta visualmente.

## Decisión

**Guards como componentes wrapper**, no loaders del router:

- `RutaProtegida` envuelve la rama `AppLayout`: sin usuario, redirige a `/login?volverA=<ruta>`, guardando la ruta intentada. Mientras se revalida un token persistido (`validandoSesion` en el store), no decide nada — evita el parpadeo a `/login` en cada refresh.
- `RutaPorRol` recibe `rolesPermitidos: RolUsuario[]` y redirige a `/dashboard` si el rol actual no está en la lista.
- Se corrigió el *fallback* de `RoleSwitch`: sin rol, ahora navega a `/login` en vez de caer en la vista de Solicitante. `RutaProtegida` ya lo hace inalcanzable en la práctica, pero dejarlo así habría sido una falla abierta ante cualquier ruta futura que olvide el guard.
- Se agregó una ruta catch-all (`path: '*'`) que redirige a `/dashboard`, que a su vez cae a `/login` vía `RutaProtegida` si no hay sesión.

**Mapa de rutas:**

| Ruta | Roles |
| --- | --- |
| `/login` | pública |
| `/dashboard`, `/solicitudes`, `/solicitudes/:id` | Administrador, Analista, Solicitante |
| `/solicitudes/nueva` | Administrador, Analista, Solicitante (ver nota) |
| `/solicitudes/:id/editar` | Administrador, Analista, Solicitante, vía `RutaPorRol` explícito |

Nota: se decidió que un Analista también puede **crear** solicitudes (no solo Administrador/Solicitante, como sugería la lectura literal del requerimiento), así que se quitó `rolesPermitidos` del ítem "Nueva solicitud" en `config/navigation.ts` para que el sidebar y el guard no diverjan.

`RutaPorRol` se cablea en `/solicitudes/:id/editar` con los tres roles explícitos: un Analista puede editar una solicitud que él mismo levantó. **La propiedad de la solicitud y la ventana de edición (solo mientras está en `REGISTRADA`) siguen siendo responsabilidad exclusiva del backend** (403/404, ver ADR-0012) — el frontend no las replica. Este es también el mecanismo reservado para el día que existan `/usuarios` y una administración de catálogos, ambas pensadas como `['Administrador']`.

## Consecuencias

- **A favor:** ninguna ruta protegida es alcanzable sin sesión ni sin el rol correcto por URL directa; la lógica de guard vive en componentes React ordinarios, legibles junto al árbol de rutas, y comparten los mismos hooks (`useRolActual`, `useAuthStore`) que ya usan las vistas.
- **En contra / trade-offs:** `RutaPorRol` hoy solo protege una ruta real (`editar`) porque `nueva` quedó abierta a los tres roles — su primer uso "con dientes" llega con `/usuarios`. Los guards de frontend son una capa de UX, no de seguridad: el backend sigue siendo la única autoridad real sobre qué puede hacer cada rol.
- **Alternativas descartadas:** loaders de `createBrowserRouter` (evitan el flash de UI protegida, pero dividen la lógica de guard entre configuración de router y componentes, y es un idioma más pesado para un boolean + un chequeo de rol).
