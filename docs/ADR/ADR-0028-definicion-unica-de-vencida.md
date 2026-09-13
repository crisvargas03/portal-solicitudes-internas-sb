# ADR-0028: Definición única de "vencida" (excluye estados finales)

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

"Vencida" se calculaba en tres lugares con dos definiciones distintas. `MapeosSolicitud.
ASolicitudResumenDto` (el flag `EstaVencida` que viaja en cada fila) y `SolicitudRepository.
ContarVencidasAsync` (el conteo del dashboard) excluían las solicitudes en un estado final
(`Estado.EsFinal`): una solicitud ya `CERRADA` con fecha de compromiso pasada no cuenta como
vencida, porque ya no hay nada pendiente. El filtro `SoloVencidas` de `GET /api/solicitudes`, en
cambio, solo miraba la fecha, sin el `!EsFinal`. El resultado: `totalVencidas` del dashboard podía
no coincidir con `totalElementos` de `GET /api/solicitudes?soloVencidas=true` para el mismo usuario,
y filtrar por "vencidas" en la tabla podía traer de vuelta filas que la propia fila marca con
`estaVencida: false`.

## Decisión

El filtro `SoloVencidas` del listado adopta la misma condición que ya usan `ContarVencidasAsync` y
`EstaVencida`: `FechaCompromiso != null && FechaCompromiso < fechaReferencia && !Estado.EsFinal`.
"Vencida" pasa a significar exactamente una cosa en toda la API: una fecha de compromiso pasada
**y todavía accionable** — no simplemente una fecha pasada en el historial.

## Consecuencias

- **A favor:** el número de vencidas del dashboard y el largo de la lista filtrada por
  `soloVencidas=true` siempre coinciden, para cualquier rol y cualquier alcance. El flag
  `estaVencida` de cada fila nunca contradice el filtro que trajo esa fila.
- **En contra / trade-offs:** cambia el comportamiento observable de un parámetro de API ya
  existente — un cliente que dependiera de `soloVencidas=true` para encontrar solicitudes cerradas
  con fecha pasada (un caso de uso "historial de incumplimientos", no mencionado en ningún
  requerimiento) dejaría de encontrarlas. No hay evidencia de que el frontend actual dependa de eso.
- **Alternativas descartadas:** mantener las dos definiciones y documentarlas como preguntas
  distintas ("venció alguna vez" vs. "vencida y pendiente"), descartada porque el propio dashboard
  y el listado necesitan mostrar el mismo número en pantallas relacionadas (p. ej., un card
  "Vencidas: 12" que al hacer clic navega al listado filtrado) — dos definiciones ahí es una fuente
  de confusión inmediata para el usuario final, no solo para quien depura la API.
