# ADR-0004: Prioridad y EstadoSolicitud como catálogos en tabla, no como enumeraciones

**Estado:** Aceptada
**Fecha:** 2026-09-09

## Contexto

`docs/architecture.md` lista `EstadoSolicitud` y `Prioridad` como enumeraciones del dominio, pero el mismo documento exige, en "Extensibilidad esperada", poder "agregar nuevos tipos de solicitud, estados o mecanismos de notificación sin reescribir la lógica de negocio", y resuelve esa exigencia declarando los catálogos (`Area`, `TipoSolicitud`) como tablas. Las dos indicaciones se contradicen para el caso de los estados.

A esto se suma que el requerimiento nunca enumera los valores de prioridad: no existe una lista cerrada que justifique fijarla en código.

Las opciones eran: dejar ambas como enumeraciones (extensibilidad solo por recompilación), dejarlas en tabla (extensibilidad por datos), o una mezcla.

## Decisión

`Prioridad` y `EstadoSolicitud` se modelan como catálogos en tabla, heredando de `CatalogoBase` igual que `Area` y `TipoSolicitud`. `Prioridad` agrega `Nivel` (severidad relativa, para ordenar listados y el dashboard) y `EstadoSolicitud` agrega `Codigo`, `Orden` y `EsFinal`.

`Codigo` es una clave estable e independiente del identificador de base de datos (`REGISTRADA`, `EN_ANALISIS`, `EN_PROGRESO`, `EN_ESPERA_SOLICITANTE`, `RESUELTA`, `CERRADA`, en `CodigosEstadoSolicitud`): es la referencia que usan los datos semilla y cualquier regla, para que los identificadores generados por la base de datos no se filtren a la lógica.

Las únicas enumeraciones que sobreviven en el dominio son `RolUsuario`, `CanalNotificacion` y `EstadoNotificacion`, persistidas como entero.

## Consecuencias

- **A favor:** agregar una prioridad o un estado es insertar una fila, sin recompilar ni migrar; los nombres visibles (incluidos los acentos de "En análisis") viven en datos y no en identificadores de C#, que no admiten espacios ni tildes; el dashboard puede agrupar y ordenar por `Nivel` y por `Orden` sin traducir enumeraciones.
- **En contra / trade-offs:** se pierde la exhaustividad en tiempo de compilación — el compilador ya no avisa cuando aparece un estado nuevo sin tratar; toda comparación pasa por `Codigo` o por identificador, y el modelo depende de que los datos semilla sean correctos; se agregan dos tablas y sus claves foráneas.
- **Alternativas descartadas:** mantener ambas como enumeraciones. Se descartó porque contradice la extensibilidad exigida explícitamente y obligaría a una recompilación y despliegue para algo que el negocio percibe como configuración. También se descartó la mezcla (estado en tabla, prioridad en enumeración) por dejar dos modelos distintos para dos conceptos equivalentes.

Este ADR reemplaza la clasificación de `EstadoSolicitud` y `Prioridad` como enumeraciones que aparecía en `docs/architecture.md`, ya actualizado.
