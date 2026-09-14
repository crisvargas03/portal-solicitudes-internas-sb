# ADR-0002: Transición de estados — no saltar directo a Cerrada

**Estado:** Aceptada
**Fecha:** 2026-09-09

## Contexto

El documento no especifica si las transiciones de estado son libres o deben seguir un orden. Existe ambigüedad sobre la diferencia entre `Resuelta` y `Cerrada`.

## Decisión

El flujo es secuencial en el tramo final: una solicitud **no puede pasar directamente a `Cerrada`** desde ningún otro estado que no sea `Resuelta`. Debe pasar primero por `Resuelta` (con su comentario de resolución obligatorio, ver [ADR-0001](ADR-0001-comentario-de-resolucion.md)) y luego a `Cerrada`.

## Consecuencias

- **A favor:** `Resuelta` representa que el trabajo técnico está terminado, mientras que `Cerrada` representa el cierre definitivo del ciclo (confirmación o cierre administrativo); la distinción queda clara y auditable en el historial de estados.
- **En contra / trade-offs:** El backend debe validar el estado actual antes de permitir la transición a `Cerrada`, agregando una regla de negocio adicional a la máquina de estados en lugar de permitir transiciones libres.
- **Alternativas descartadas:** Permitir transiciones libres entre cualquier estado hacia `Cerrada`. Se descartó porque eliminaría la distinción semántica entre "trabajo resuelto" y "ciclo cerrado", y dificultaría exigir el comentario de resolución de forma consistente.
