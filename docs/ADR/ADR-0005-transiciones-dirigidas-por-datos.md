# ADR-0005: Las transiciones de estado se declaran en datos, no en código

**Estado:** Aceptada
**Fecha:** 2026-09-09

## Contexto

[ADR-0002](ADR-0002-transicion-de-estados.md) fija la regla de negocio: no se puede pasar a `Cerrada` desde ningún estado que no sea `Resuelta`. [ADR-0001](ADR-0001-comentario-de-resolucion.md) fija que el comentario es obligatorio en la transición hacia `Resuelta`. Y el requerimiento agrega una tercera regla: una solicitud cerrada solo la puede reabrir un Administrador o un Analista.

Con `EstadoSolicitud` convertido en catálogo en tabla ([ADR-0004](ADR-0004-catalogos-en-tabla-en-lugar-de-enumeraciones.md)), esas tres reglas ya no se pueden expresar como un `switch` exhaustivo sobre una enumeración. Había que decidir dónde viven: codificadas contra `Codigo`, deducidas de banderas del propio estado (`Orden`, `EsFinal`), o declaradas como filas.

## Decisión

Las transiciones legítimas se declaran en la tabla `TransicionPermitida` (`EstadoOrigenId`, `EstadoDestinoId`, `RequiereComentario`, `Activo`), con los roles autorizados en la tabla hija `TransicionPermitidaRol` (`TransicionPermitidaId`, `Rol`). Las tres reglas quedan expresadas como datos:

- la única fila con destino `CERRADA` tiene origen `RESUELTA` (ADR-0002);
- las filas con destino `RESUELTA` llevan `RequiereComentario = true`, y ese comentario es el comentario de resolución (ADR-0001);
- las filas de reapertura con origen `CERRADA` solo listan `Administrador` y `Analista`.

Las entidades de `Domain` son definiciones de datos sin comportamiento: `Solicitud` no valida ni ejecuta la transición. La capa de aplicación consulta `TransicionPermitida`, decide si la transición procede y, en la misma operación, actualiza `Solicitud.EstadoId` y agrega el registro en `HistorialEstado`. Los roles se modelan como tabla hija y no como enumeración de banderas, para no depender de valores compuestos (`docs/conventions.md` prohíbe los números mágicos).

## Consecuencias

- **A favor:** ADR-0002 sigue vigente sin cambios — solo cambia su mecanismo, no la regla; agregar un estado o un camino nuevo es insertar filas, cerrando el círculo de la extensibilidad de ADR-0004; `RequiereComentario` y los roles autorizados quedan configurables por transición en lugar de repartidos en condicionales; la máquina de estados completa es consultable y auditable con una sola consulta, y se puede exponer al frontend para que solo ofrezca las transiciones válidas.
- **En contra / trade-offs:** la corrección del flujo pasa a depender de que los datos semilla sean correctos, y un error ahí no lo detecta el compilador sino una prueba o el uso; se agregan dos tablas y un `JOIN` en cada cambio de estado; `Solicitud` deja de ser autosuficiente para validar su propio ciclo de vida, así que nada impide escribir `solicitud.EstadoId = otroEstado` sin pasar por la validación ni dejar rastro en el historial — la disciplina queda del lado de los casos de uso.
- **Alternativas descartadas:** codificar las reglas contra `Codigo` (`if (destino.Codigo == CERRADA && origen.Codigo != RESUELTA)`), descartada porque anula el motivo de haber llevado los estados a una tabla; y deducir el flujo de banderas en `EstadoSolicitud` (`Orden`, `EsFinal`), descartada porque impone un flujo estrictamente lineal y no permite expresar la reapertura ni los permisos por rol.
