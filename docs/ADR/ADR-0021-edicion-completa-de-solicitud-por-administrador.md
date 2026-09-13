# ADR-0021: Edición completa de Solicitud por Administrador, separada de la edición parcial

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

ADR-0012 restringe `PATCH /api/solicitudes/{id}` al propio Solicitante o a un Administrador, y solo mientras la solicitud está en `REGISTRADA` — una vez que entra a análisis, el contenido se congela. El Administrador necesita, sin embargo, poder corregir título, descripción, tipo, prioridad y área en cualquier momento del ciclo de vida (por ejemplo, una solicitud mal clasificada que ya está `En progreso`). Sobrecargar el `PATCH` existente para que el Administrador se salte la regla de `REGISTRADA` mezclaría dos semánticas distintas bajo el mismo endpoint y complicaría su validador y su handler con un `if (esAdministrador) { ignorar la regla de estado }`.

Quedaba además por decidir si esta edición debía poder tocar `Estado` o `UsuarioAsignadoId`, y si el hecho de editar una solicitud ya en análisis debía dejar rastro.

## Decisión

Se agrega `PUT /api/solicitudes/{id}` (`[Authorize(Roles="Administrador")]`), separado del `PATCH` existente. `AdminActualizarSolicitudCommand` recibe los cinco campos (`Titulo`, `Descripcion`, `TipoSolicitudId`, `PrioridadId`, `AreaId`) como requeridos, no opcionales — es una edición completa, no parcial, coherente con la semántica de `PUT`. No valida el estado de la solicitud ni distingue Solicitante vs. Administrador (el `[Authorize]` del controlador ya deja pasar solo a Administrador). No toca `Estado` ni `UsuarioAsignadoId`: esos siguen su propio camino gobernado por reglas (`PATCH .../estado` contra la máquina de estados de ADR-0005, `PATCH .../asignacion` con las reglas de ADR-0012).

Como la solicitud puede ahora editarse después de `REGISTRADA` — algo que antes era imposible —, el handler escribe automáticamente un `Comentario` interno ("Solicitud editada por Administrador.") en la misma transacción que la actualización, para que el historial visible no cambie de contenido sin dejar rastro.

## Consecuencias

- **A favor:** el `PATCH` original y su regla de `REGISTRADA` quedan intactos y siguen expresando "edición del propio Solicitante mientras nadie ha tocado la solicitud"; el nuevo `PUT` expresa "corrección administrativa en cualquier momento" sin condicionales cruzados entre ambos handlers; las transiciones de estado y la asignación siguen siendo la única puerta para cambiar esos dos campos, sin atajos.
- **En contra / trade-offs:** dos endpoints con nombres de comando parecidos (`ActualizarSolicitudCommand` y `AdminActualizarSolicitudCommand`) que un desarrollador nuevo debe aprender a distinguir; el comentario de auditoría automático aparece en el historial como si lo hubiera escrito el Administrador explícitamente, aunque es generado por el sistema.
- **Alternativas descartadas:** permitir que el `PUT` también cambie `Estado`/`UsuarioAsignadoId` en una sola llamada — descartada porque volvería bypaseable la máquina de estados dirigida por datos (ADR-0005) y las reglas finas de asignación (ADR-0012) con solo tener rol Administrador; omitir el comentario de auditoría — descartada porque el `PATCH` original nunca permite editar fuera de `REGISTRADA`, así que no hay precedente de "edición silenciosa en pleno análisis" que datos históricos ya asuman.
