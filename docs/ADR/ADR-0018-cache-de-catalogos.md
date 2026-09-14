# ADR-0018: Caché de catálogos vía React Query

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

Los selects de filtros (`SolicitudFilters`) y del formulario de solicitud (`SolicitudForm`) leían `src/mocks/catalogos.ts` o tenían `<option>` hardcodeados. Los cuatro catálogos correspondientes ya existen en el backend — `GET /api/areas`, `/api/tipos-solicitud`, `/api/prioridades`, `/api/estados-solicitud` — y los cuatro son `[Authorize]`: solo responden con una sesión activa.

Swagger no declara la forma de la respuesta de ninguno (no hay schemas de éxito documentados); se confirmó contra la API viva con un usuario semilla:

| Endpoint | Forma de `datos` |
| --- | --- |
| `GET /api/areas` | `{ id, nombre }[]` |
| `GET /api/tipos-solicitud` | `{ id, nombre }[]` — **sin** `descripcion` ni `activo` |
| `GET /api/prioridades` | `{ id, nombre, nivel }[]` |
| `GET /api/estados-solicitud` | `{ id, codigo, nombre, orden, esFinal }[]` |

Los tipos previos del frontend (`Area`, `TipoSolicitud`, `Prioridad`, `EstadoSolicitud`) declaraban un campo `activo: boolean` que ninguno de estos cuatro endpoints manda (la API ya filtra por activos antes de responder), y `TipoSolicitud` declaraba además `descripcion?: string`, tampoco presente en la respuesta real.

## Decisión

Se wiring los **cuatro** catálogos, no solo los dos que el pedido original nombraba (áreas y tipos de solicitud): `SolicitudFilters` filtra también por estado y prioridad, y dejarlos en mocks habría significado que la mitad de cada fila de filtros seguía en datos hardcodeados.

`hooks/queries/useCatalogos.ts` — cuatro hooks (`useAreas`, `useTiposSolicitud`, `usePrioridades`, `useEstadosSolicitud`), siguiendo la convención `hooks/queries/<entidad>.ts` ya establecida por `useSolicitudes.ts`:

- `staleTime: Infinity` y `gcTime` de 24 horas: estos catálogos cambian rarísimo (son configuración, no datos operativos), así que no hay razón para refetchear en cada montaje o navegación.
- `enabled` condicionado a que haya token en el `authStore`, porque los cuatro endpoints son `[Authorize]`.
- Query keys inline (`['catalogos', 'areas']`, etc.), sin introducir una factory de keys — no hay precedente de eso en el proyecto y sería un patrón nuevo a medias.

Los tipos de `src/types/` se recortaron para reflejar exactamente lo que la API manda (sin `activo` en los cuatro, sin `descripcion` en `TipoSolicitud`) en vez de mantener campos que nunca llegan por la red.

`src/mocks/catalogos.ts` **no se eliminó**: `mocks/solicitudes.ts` sigue dependiendo de su lista `USUARIOS` y de `obtenerEstadoPorCodigo` mientras las solicitudes sigan siendo mock. Solo se dejó de importar de ahí las listas de catálogo que ya tienen endpoint real.

## Consecuencias

- **A favor:** los filtros y el formulario muestran los catálogos reales del backend sin refetch innecesario; los tipos del frontend ya no prometen campos (`activo`, `descripcion`) que la API nunca envía, lo que habría sido un desajuste silencioso detectable solo en runtime.
- **En contra / trade-offs:** si algún día se agrega administración de catálogos (crear/desactivar un área, por ejemplo) con invalidación desde otra pantalla, `staleTime: Infinity` exige invalidar la query a mano en esa mutación — no hay refetch automático por tiempo que lo cubra.
- **Alternativas descartadas:** wirear solo áreas y tipos de solicitud (deja estado y prioridad —usados por los mismos filtros— en mocks, una inconsistencia visible en la misma fila de UI).
