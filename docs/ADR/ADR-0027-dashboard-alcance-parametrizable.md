# ADR-0027: Dashboard con forma única por rol y alcance parametrizable

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

El dashboard de Analista necesita mostrar "mi carga de trabajo" — abiertas, vencidas, por
prioridad — pero el alcance de un Analista (ADR-0012) es una unión: lo suyo más lo sin asignar.
Sin un corte adicional, `GET /api/dashboard/resumen` para un Analista mezclaría en sus métricas
personales solicitudes que ni siquiera son suyas todavía, lo cual contradice la etiqueta "Mi carga"
que ya tiene la UI. Quedaba además por decidir si el dashboard debía tener una forma de respuesta
distinta por rol, o una sola forma con los números ya recortados.

## Decisión

`GET /api/dashboard/resumen` gana el mismo parámetro `asignacion=todas|asignadas|disponibles` que
[ADR-0026](ADR-0026-toggle-de-asignacion-y-orden-en-listado.md) definió para el listado, con
idéntica semántica aditiva sobre el alcance por rol. El dashboard de Analista lo llama con
`asignacion=asignadas` para que "Mi carga" sea literalmente solo lo suyo.

La forma de la respuesta (`ResumenDashboardDto`) es **una sola para los tres roles** — la alternativa
de una forma distinta por rol se descarta por simplicidad, dado que el recorte por alcance ya hace
todo el trabajo de adaptar los números sin necesidad de un contrato distinto. Se agrega un campo
`TotalSinAsignar` (conteo de sin responsable, dentro del alcance del usuario, sin aplicar el corte
de `asignacion` activo) para que el Analista pueda mostrar "hay N disponibles para tomar" en su
propio dashboard sin una segunda llamada a `/api/solicitudes?asignacion=disponibles`.

Internamente, las cuatro consultas de conteo pasan de recibir un `AlcanceSolicitudes` suelto a un
`CriterioDashboard` (alcance + los mismos dos campos aditivos que `FiltroSolicitudes`), para que el
listado y el dashboard compartan literalmente la misma forma de estrechar y nunca puedan discrepar
sobre qué significa "asignadas a mí" o "disponibles".

## Consecuencias

- **A favor:** un solo contrato de respuesta para los tres roles, más simple de documentar y de
  consumir desde el frontend (un solo tipo `ResumenDashboard`). El dashboard de Analista puede
  mostrar tanto su carga como el tamaño del pool disponible sin una llamada adicional.
- **En contra / trade-offs:** `CriterioDashboard` es un tipo nuevo que se parece bastante a
  `FiltroSolicitudes` pero no es el mismo (el dashboard no tiene columnas de texto, fechas ni
  ordenamiento) — riesgo de que diverjan con el tiempo si no se revisan juntos.
- **Alternativas descartadas:** una forma de respuesta distinta por rol (p. ej.
  `ResumenDashboardAnalistaDto` con campos propios) fue descartada porque el recorte por alcance ya
  resuelve el problema sin duplicar el contrato ni el código de mapeo.
