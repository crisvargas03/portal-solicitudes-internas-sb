# ADR-0006: La evidencia se modela como entidad `Adjunto`, no como campos de `Solicitud`

**Estado:** Aceptada
**Fecha:** 2026-09-09

## Contexto

El requerimiento pide "adjuntar una referencia en texto o URL de evidencia", y aclara que **no hay almacenamiento físico de archivos**. El modelo de datos mínimo no incluye una entidad para esto, así que quedaba abierto si la evidencia es un par de campos en `Solicitud` o una entidad propia.

## Decisión

Se agrega la entidad `Adjunto` (`SolicitudId`, `UsuarioId`, `Descripcion`, `Url`, `Fecha`), como colección hija de `Solicitud`. Solo se guarda el texto y la dirección: no se descarga ni se almacena el contenido apuntado por la URL.

## Consecuencias

- **A favor:** una solicitud puede acumular varias evidencias a lo largo de su ciclo de vida, que es el caso real de un helpdesk; queda registrado quién aportó cada referencia y cuándo, lo que la hace parte de la trazabilidad junto con `HistorialEstado` y `Comentario`; y no se agregan campos que quedarían nulos en la mayoría de las solicitudes.
- **En contra / trade-offs:** es una tabla más allá del modelo mínimo documentado, y el detalle de una solicitud necesita una consulta o un `Include` adicional.
- **Alternativas descartadas:** dos campos `ReferenciaEvidencia` y `UrlEvidencia` en `Solicitud`. Se descartó porque limita a una sola evidencia por solicitud y pierde autor y fecha, dejando un dato sin trazabilidad en un sistema cuyo objetivo declarado es auditar el ciclo de vida de la solicitud.
