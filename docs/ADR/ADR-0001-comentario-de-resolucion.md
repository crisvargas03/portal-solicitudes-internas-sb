# ADR-0001: Comentario de resolución reutiliza el comentario de HistorialEstado

**Estado:** Aceptada
**Fecha:** 2026-09-09

## Contexto

El documento exige que no se pueda cerrar una solicitud sin comentario de resolución, pero no aclara si es un campo propio en `Solicitud` o si reutiliza el comentario de las transiciones de estado.

## Decisión

No se agrega un campo nuevo a `Solicitud`. Se reutiliza el mismo `comentario` que ya se registra en cada transición (`HistorialEstado.comentario`). Al pasar al estado `Resuelta`, ese comentario es obligatorio y actúa como el "comentario de resolución".

## Consecuencias

- **A favor:** No se duplica información entre `Solicitud` y `HistorialEstado`; el historial de comentarios queda centralizado en un solo lugar.
- **En contra / trade-offs:** Para consultar el comentario de resolución de una solicitud, se debe buscar el último registro de `HistorialEstado` con `estadoNuevo = Resuelta`, en lugar de leerlo directamente de `Solicitud`.
- **Alternativas descartadas:** Agregar un campo `comentarioResolucion` directamente en `Solicitud`. Se descartó porque duplicaría información ya capturada en `HistorialEstado` y obligaría a mantener ambos campos sincronizados.
