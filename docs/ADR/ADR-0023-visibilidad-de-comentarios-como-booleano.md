# ADR-0023: Visibilidad de comentarios como `bool EsInterno`, filtrada en el servidor

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

`docs/CLAUDE.md` describe `Comentario` con un campo "visibility" que "distingue interno de
visible-para-el-solicitante", pero no dice cómo modelarlo, y no existía un ADR que lo resolviera
aunque la implementación ya llevaba varios commits en producción. La visibilidad quedaba, en la
práctica, decidida en código sin quedar registrada: exactamente el caso que `docs/CLAUDE.md` pide
marcar y documentar antes de tocar código.

Las opciones para el campo eran un `bool` (dos valores posibles) o un catálogo en tabla como
`EstadoSolicitud`/`Prioridad` (ver [ADR-0004](ADR-0004-catalogos-en-tabla-en-lugar-de-enumeraciones.md)).
Independientemente del tipo de dato, quedaba abierto además dónde se aplica el filtro (¿el
servidor nunca envía los comentarios internos a un Solicitante, o los envía y el frontend decide
no mostrarlos?) y qué pasa si un Solicitante intenta crear un comentario interno (¿se rechaza con
400, o se ignora la bandera?).

## Decisión

`Comentario.EsInterno` es un `bool`: `true` oculta el comentario a quien tiene rol Solicitante,
`false` es visible para cualquiera con acceso a la solicitud. El filtrado ocurre en el servidor,
no en el cliente: `ObtenerDetalleSolicitudQueryHandler` calcula
`incluirComentariosInternos = Rol is Administrador or Analista` y `MapeosSolicitud.ASolicitudDetalleDto`
excluye los comentarios internos de la lista antes de serializar la respuesta — un Solicitante
jamás recibe ese JSON, no es solo un `esInterno` que el frontend elige no pintar.

Al crear un comentario, `CrearComentarioCommandHandler` hace
`esInterno = !esSolicitante && command.EsInterno`: si un Solicitante envía `esInterno: true`, se
guarda igual como público en lugar de devolver un error de validación.

## Consecuencias

- **A favor:** un Solicitante no puede, bajo ninguna circunstancia de cliente (inspeccionar la
  red, un bug de UI), leer el contenido de un comentario interno — la garantía vive donde importa.
  Downgrade silencioso en la creación evita que un formulario mal armado en el frontend (o un
  llamado directo a la API) rompa el flujo con un 400 por un campo que, para ese rol, no tiene
  sentido exponer como error.
- **En contra / trade-offs:** un `bool` no admite un tercer nivel de visibilidad si aparece más
  adelante (p. ej. "solo Administrador"); agregarlo sería un cambio de tipo, no solo un valor
  nuevo en una tabla, a diferencia del patrón de ADR-0004. El downgrade silencioso también oculta
  a quien integra contra la API que su intención (`esInterno: true`) fue ignorada — no hay señal
  de que el servidor decidió otra cosa.
- **Alternativas descartadas:** una tabla `VisibilidadComentario` como las de ADR-0004, descartada
  porque, a diferencia de `EstadoSolicitud`/`Prioridad`, este es un campo binario sin nombres
  visibles ni orden que el negocio vaya a necesitar administrar — agregar una tabla ahí sería la
  complejidad que ADR-0004 evita en el otro sentido. Devolver 400 en vez de forzar público,
  descartada por consistencia con la postura general de la API ante acciones no permitidas por rol
  (ver [ADR-0012](ADR-0012-alcance-de-autorizacion-por-rol.md), que prefiere una respuesta que no
  interrumpa el flujo — allá un 404 en vez de 403 — sobre un error que expone la regla al cliente).
