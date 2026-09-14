# ADR-0030: Las filas de listado espejan `SolicitudResumenDto` (sin ids planos)

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

El tipo `Solicitud` del frontend (`types/solicitud.ts`) traía tanto ids planos (`prioridadId`,
`areaId`, `usuarioSolicitanteId`, `usuarioAsignadoId`, ...) como los objetos anidados opcionales
(`prioridad?`, `area?`, ...), y una `descripcion` obligatoria. Era la forma que tenía sentido para
`MOCK_SOLICITUDES`, construido a mano en el frontend con control total de sus campos. El DTO real
que devuelve el servidor, `SolicitudResumenDto`, nunca tuvo esos ids planos — sólo los objetos
anidados (`estado`, `prioridad`, `area`, `tipoSolicitud`, `solicitante`, `asignado`) — ni trae
`descripcion` (eso vive solo en `SolicitudDetalleDto`, el detalle). Conectar el listado real a ese
tipo sin ajustarlo habría dejado cada lectura de un id plano devolviendo `undefined` en silencio,
sin ningún error de compilación que lo señalara.

## Decisión

`types/solicitud.ts` se reescribe para reflejar `SolicitudResumenDto` campo por campo: sin ids
planos, con `solicitante`/`asignado` (no `usuarioSolicitante`/`usuarioAsignado`) y sin
`descripcion`. Cualquier lugar que hoy filtra o lee por un id plano (p. ej. el dashboard de
Analista filtrando por `solicitud.prioridadId`) se reescribe para leer el objeto anidado
(`solicitud.prioridad.id`). Es un cambio deliberadamente disruptivo en tiempo de compilación:
el objetivo es que `tsc` señale cada consumidor que asumía un campo que la API real nunca envía,
en vez de que seguir funcionando "por accidente" con valores `undefined`.

## Consecuencias

- **A favor:** el tipo del frontend dice la verdad sobre lo que la API devuelve; un futuro cambio en
  `SolicitudResumenDto` que quite o renombre un campo vuelve a fallar la compilación en el lugar
  correcto, en vez de fallar en producción con un valor `undefined` silencioso.
- **En contra / trade-offs:** rompe en compilación varios consumidores existentes que sí asumían los
  ids planos, obligando a tocar cada uno en el mismo cambio (alcance más grande de lo que el nombre
  del archivo sugiere).
- **Alternativas descartadas:** mantener ambas formas (ids planos opcionales además de los objetos
  anidados) para no romper nada, descartada porque perpetúa la causa raíz del problema — un campo
  que el compilador cree que existe pero que el servidor nunca envía.
