# ADR-0026: Toggle de asignación y ordenamiento explícito en `GET /api/solicitudes`

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

[ADR-0012](ADR-0012-alcance-de-autorizacion-por-rol.md) fija el alcance de un Analista como "las
asignadas a sí mismo, más las que no tienen responsable todavía" — una unión. La cola de Analista
diseñada en el frontend, sin embargo, necesita mostrar esas dos mitades por separado, con un toggle
"Asignadas a mí" / "Disponibles para tomar" y contadores propios para cada una. El listado tampoco
tenía ningún parámetro de ordenamiento: `SolicitudRepository` traía el orden fijo en
`FechaCreacion DESC`, lo que bastaba para una tabla que ordenaba en memoria (`useTablaLocal`) pero
no alcanza en cuanto la paginación pasa al servidor, ni cubre "prioridad, luego vencimiento" que
pide la cola del Analista.

## Decisión

`GET /api/solicitudes` gana `asignacion=todas|asignadas|disponibles` (default `todas`) y
`orden=fechaCreacion|codigo|titulo|prioridad|urgencia` + `direccion=asc|desc` (default
`fechaCreacion desc`), ambos resueltos server-side con una whitelist — nunca una columna arbitraria
del cliente.

`asignacion` se traduce a dos campos aditivos en `FiltroSolicitudes`
(`AsignadasAUsuarioId`, `SoloSinAsignar`) que se aplican **encima** de las cláusulas de alcance ya
existentes, nunca en su lugar: el handler arma `AsignadasAUsuarioId = usuarioActual.Id` solo si pide
`asignadas`, y `SoloSinAsignar = true` solo si pide `disponibles`. Como son condiciones adicionales
con `AND` sobre lo que el alcance ya permite ver, es estructuralmente imposible que este parámetro
amplíe la visibilidad de nadie — como mucho, la deja igual.

`orden=urgencia` no expone una dirección configurable: siempre es prioridad descendente y, a
igual prioridad, fecha de compromiso ascendente con los `null` al final (sin fecha, al final de la
cola). Las demás columnas sí respetan `direccion`.

## Consecuencias

- **A favor:** la cola de Analista puede pedir cada mitad por separado con una sola llamada por
  pestaña, con contadores server-side reales (`totalElementos`) en vez de contar arrays ya
  truncados por paginación. El ordenamiento en memoria (`useTablaLocal`) deja de ser necesario y se
  elimina del frontend sin perder ninguna columna ordenable.
- **En contra / trade-offs:** un parámetro más que documentar y mantener sincronizado entre el
  listado y el dashboard (ver [ADR-0027](ADR-0027-dashboard-alcance-parametrizable.md), que reutiliza
  el mismo enum). `orden=urgencia` con dirección fija es una excepción a la regla general del
  parámetro `direccion`, que hay que explicar en la documentación de la API.
- **Alternativas descartadas:** exponer un parámetro de ordenamiento libre (`orden=FechaCompromiso
  desc`, tipo SQL) fue descartado por seguridad — permitiría ordenar por columnas no pensadas para
  eso y complica la validación; una whitelist de enum es más segura y de todas formas cubre todo lo
  que el diseño de UI necesita hoy.
