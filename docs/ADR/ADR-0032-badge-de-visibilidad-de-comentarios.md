# ADR-0032: Badge de visibilidad por comentario, solo en vista Admin/Analista

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

Con el checkbox "Comentario interno" habilitado para Administrador/Analista (ver ADR-0023),
la lista de comentarios del detalle necesita distinguir a simple vista cuáles son internos y
cuáles públicos. El único precedente (`SolicitudDetail.tsx`, antes de este cambio) marcaba
`esInterno` con `<Badge tono="advertencia">` (ámbar) y no marcaba nada para los públicos.

## Decisión

`Interno` usa `tono="neutral"` (gris) y `Público` usa `tono="navy"` — ambos ya definidos en
`ui/tono.ts`, sin agregar clases nuevas. Los dos badges se muestran siempre en pares por
comentario, pero únicamente cuando `puedeComentarInterno` (Administrador/Analista); el
Solicitante nunca recibe comentarios internos en la respuesta del servidor (ADR-0023), así que
para su vista el badge no aporta información y no se renderiza.

## Consecuencias

- **A favor:** `advertencia` (ámbar) queda libre para lo que ya la usa con sentido de alarma
  (`StatusBadge`/`PriorityBadge`); la visibilidad de un comentario es un matiz informativo, no
  una alerta. Mostrar ambos estados (no solo "Interno") hace la lista escaneable sin tener que
  interpretar la ausencia de badge.
- **En contra / trade-offs:** cada comentario en la vista Admin/Analista lleva una pill más,
  aumentando el ruido visual de la lista frente a la versión anterior (que no marcaba los
  públicos).
- **Alternativas descartadas:** mantener `advertencia` para Interno sin agregar badge a
  Público, descartada por competir visualmente con las alarmas reales de la pantalla
  (vencimiento, prioridad crítica) y por dejar la ausencia de badge como única señal de
  "público", menos explícita que un badge propio.
