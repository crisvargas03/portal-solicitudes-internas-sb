# ADR-0012: Alcance de autorización por rol más allá de las transiciones

**Estado:** Aceptada
**Fecha:** 2026-09-10

## Contexto

[ADR-0005](ADR-0005-transiciones-dirigidas-por-datos.md) resuelve qué rol puede ejecutar cada cambio de estado, con datos. `docs/open-decisions.md` (entrada #5, retirada por este ADR) dejaba abierto el resto: qué solicitudes ve un Analista ("asignadas o disponibles para gestión" admite más de una lectura), si un Solicitante puede editar su solicitud y hasta cuándo, y quién puede reasignar.

## Decisión

**Visibilidad de listado y dashboard** (`AlcanceSolicitudesFactory`, aplicada igual en ambos para que nunca discrepen):

| Rol | Ve |
| --- | --- |
| Solicitante | Solo sus propias solicitudes (`UsuarioSolicitanteId` forzado al usuario autenticado) |
| Analista | Las asignadas a sí mismo, más las que no tienen responsable todavía |
| Administrador | Todas |

El detalle de una solicitud ajena a un Solicitante devuelve **404**, no 403: no debe poder distinguir "no existe" de "no es mía", lo que filtraría el rango de códigos en uso.

**Edición parcial** (`PATCH /api/solicitudes/{id}`): el propio Solicitante o un Administrador, y solo mientras la solicitud sigue en `REGISTRADA` — una vez que entra a análisis, el contenido se congela y cualquier cambio pasa a ser un comentario de seguimiento, no una edición.

**Asignación** (`PATCH /api/solicitudes/{id}/asignacion`): Administrador asigna o reasigna sin restricción. Un Analista solo puede **reclamar para sí mismo** una solicitud que todavía no tiene responsable; reasignar una ya asignada, o asignarla a un tercero, es exclusivo de Administrador. Esta regla no se deriva de la tabla anterior por sí sola — es la consecuencia necesaria de que un Analista vea solicitudes sin asignar pero no pueda actuar sobre ellas de otro modo, y se decide aquí explícitamente.

## Consecuencias

- **A favor:** el Analista tiene una cola de trabajo accionable (lo suyo más lo disponible) sin poder pisar el trabajo de otro Analista; el Administrador conserva control total de la asignación como árbitro; la regla de "editar solo en REGISTRADA" evita que el contenido de una solicitud cambie después de que alguien ya la analizó.
- **En contra / trade-offs:** un Analista no puede ver la cola de un colega para ayudar si hace falta — solo Administrador tiene esa vista completa; el 404 en vez de 403 para solicitudes ajenas es coherente con no filtrar existencia, pero puede confundir a quien depura contra la API esperando un 403.
- **Alternativas descartadas:** que cada Analista vea solo lo asignado a sí mismo (sin las sin asignar), descartada porque entonces nadie podría tomar una solicitud nueva sin que un Administrador la asignara primero, lo que vuelve al Administrador un cuello de botella para cada solicitud entrante.
